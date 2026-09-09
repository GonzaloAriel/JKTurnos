using System;
using System.Collections.Generic;
using System.Linq;
using JKTurnos.Models.Entities;
using Newtonsoft.Json;

namespace JKTurnos.Services
{
    /// <summary>
    /// D-050 / M-01 (Concepto B, evolucion): horario de atencion POR DIA con
    /// multiples rangos (turno partido). Fuente unica de verdad para parsear y
    /// normalizar ConfiguracionTenant.HorarioAtencion (JSON). Compartido entre
    /// ConfiguracionService (persistencia) y VidrieraService (lectura publica,
    /// que NO puede usar BaseRepository por el claim TenantId).
    ///
    /// Semantica (M-01): INFORMATIVO y no bloqueante. Dia sin entrada en el JSON
    /// = no se atiende. Si no hay JSON se degrada a DiasLaborales + HoraApertura +
    /// HoraCierre (legacy) y, si tampoco, a Lun-Sab 08:00-19:30 (default historico).
    /// Formato JSON: [{"Dia":1,"Rangos":[{"Inicio":"08:00","Fin":"12:00"}]}].
    /// Dia: 1=Lun .. 7=Dom.
    /// </summary>
    public static class HorarioAtencionHelper
    {
        public static DayOfWeek DayOfWeekDe(int num)
        {
            return num == 7 ? DayOfWeek.Sunday : (DayOfWeek)num;
        }

        public static int NumDia(DayOfWeek dow)
        {
            return dow == DayOfWeek.Sunday ? 7 : (int)dow;
        }

        private static bool ParseHora(string valor, out TimeSpan hora)
        {
            return TimeSpan.TryParse(valor, out hora);
        }

        /// <summary>
        /// Lista Lun..Dom por defecto: Lunes a Sabado 08:00-19:30.
        /// </summary>
        public static List<HorarioAtencionDia> DefaultLista()
        {
            var lista = new List<HorarioAtencionDia>();
            for (int i = 1; i <= 6; i++)
            {
                lista.Add(new HorarioAtencionDia
                {
                    Dia = i,
                    Rangos = new List<RangoAtencion>
                    {
                        new RangoAtencion { Inicio = "08:00", Fin = "19:30" }
                    }
                });
            }
            return lista;
        }

        /// <summary>
        /// JSON default (Lun-Sab 08:00-19:30) para seeds y pre-fill del editor.
        /// </summary>
        public static string DefaultJson()
        {
            return Serializar(DefaultLista());
        }

        /// <summary>
        /// Parsea y NORMALIZA un JSON: valida 1..7 por dia, horarios "HH:mm" y
        /// Fin &gt; Inicio; dedupe y ordena rangos y dias; descarta dias sin rangos.
        /// Devuelve la lista normalizada (vacia si no hay nada valido).
        /// </summary>
        public static List<HorarioAtencionDia> ParsearLista(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<HorarioAtencionDia>();

            List<HorarioAtencionDia> entrada;
            try
            {
                entrada = JsonConvert.DeserializeObject<List<HorarioAtencionDia>>(json);
            }
            catch
            {
                return new List<HorarioAtencionDia>();
            }

            if (entrada == null)
                return new List<HorarioAtencionDia>();

            var resultado = new List<HorarioAtencionDia>();

            foreach (var d in entrada.OrderBy(x => x.Dia))
            {
                if (d == null || d.Dia < 1 || d.Dia > 7 || d.Rangos == null)
                    continue;

                var claves = new HashSet<string>();
                var rangos = new List<RangoAtencion>();

                foreach (var r in d.Rangos)
                {
                    if (r == null)
                        continue;

                    TimeSpan ini, fin;
                    if (!ParseHora(r.Inicio, out ini) || !ParseHora(r.Fin, out fin))
                        continue;
                    if (fin <= ini)
                        continue;

                    var iniS = ini.ToString(@"hh\:mm");
                    var finS = fin.ToString(@"hh\:mm");
                    var clave = iniS + "-" + finS;
                    if (!claves.Add(clave))
                        continue;

                    rangos.Add(new RangoAtencion { Inicio = iniS, Fin = finS });
                }

                if (rangos.Count > 0)
                {
                    resultado.Add(new HorarioAtencionDia { Dia = d.Dia, Rangos = rangos });
                }
            }

            return resultado;
        }

        /// <summary>
        /// JSON canonico de una lista ("" si no hay dias). Se persiste tal cual.
        /// </summary>
        public static string Serializar(List<HorarioAtencionDia> dias)
        {
            if (dias == null || dias.Count == 0)
                return "";
            return JsonConvert.SerializeObject(dias);
        }

        /// <summary>
        /// JSON canonico a partir de un valor del form ("" si nada valido).
        /// </summary>
        public static string Normalizar(string json)
        {
            return Serializar(ParsearLista(json));
        }

        /// <summary>
        /// Horario EFECTIVO del negocio: JSON -> legacy (DiasLaborales + HoraApertura
        /// + HoraCierre) -> default Lun-Sab 08:00-19:30. Ordenado Lun..Dom.
        /// </summary>
        public static List<HorarioAtencionDia> ResolverLista(ConfiguracionTenant config)
        {
            if (config != null && !string.IsNullOrWhiteSpace(config.HorarioAtencion))
            {
                var desdeJson = ParsearLista(config.HorarioAtencion);
                if (desdeJson.Count > 0)
                    return desdeJson;
            }

            // Legacy: DiasLaborales + franja unica. Se traduce a un rango por dia.
            if (config != null
                && config.HoraApertura.HasValue
                && config.HoraCierre.HasValue
                && config.HoraCierre.Value > config.HoraApertura.Value
                && !string.IsNullOrWhiteSpace(config.DiasLaborales))
            {
                var lista = new List<HorarioAtencionDia>();
                foreach (var parte in config.DiasLaborales.Split(','))
                {
                    int num;
                    if (!int.TryParse(parte.Trim(), out num) || num < 1 || num > 7)
                        continue;

                    lista.Add(new HorarioAtencionDia
                    {
                        Dia = num,
                        Rangos = new List<RangoAtencion>
                        {
                            new RangoAtencion
                            {
                                Inicio = config.HoraApertura.Value.ToString(@"hh\:mm"),
                                Fin = config.HoraCierre.Value.ToString(@"hh\:mm")
                            }
                        }
                    });
                }

                if (lista.Count > 0)
                    return lista.OrderBy(x => x.Dia).ToList();
            }

            return DefaultLista();
        }

        /// <summary>
        /// Horario efectivo indexado por DayOfWeek (claves con al menos un rango).
        /// </summary>
        public static Dictionary<DayOfWeek, List<RangoAtencion>> Resolver(ConfiguracionTenant config)
        {
            var resultado = new Dictionary<DayOfWeek, List<RangoAtencion>>();

            foreach (var d in ResolverLista(config))
            {
                if (d == null || d.Rangos == null || d.Rangos.Count == 0)
                    continue;
                resultado[DayOfWeekDe(d.Dia)] = d.Rangos;
            }

            return resultado;
        }

        /// <summary>
        /// Minutos de atencion de un dia (suma de sus rangos). 0 si el dia no se atiende.
        /// </summary>
        public static int MinutosDelDia(Dictionary<DayOfWeek, List<RangoAtencion>> horario, DayOfWeek dia)
        {
            List<RangoAtencion> rangos;
            if (horario == null || !horario.TryGetValue(dia, out rangos))
                return 0;

            int total = 0;
            foreach (var r in rangos)
            {
                TimeSpan ini, fin;
                if (!ParseHora(r.Inicio, out ini) || !ParseHora(r.Fin, out fin))
                    continue;
                if (fin <= ini)
                    continue;
                total += (int)(fin - ini).TotalMinutes;
            }
            return total;
        }

        /// <summary>
        /// Limites de la semana completa (min Inicio / max Fin) para la Vidriera
        /// publica. Devuelve null si no hay horario.
        /// </summary>
        public static void LimitesSemana(
            Dictionary<DayOfWeek, List<RangoAtencion>> horario,
            out TimeSpan? desde,
            out TimeSpan? hasta)
        {
            desde = null;
            hasta = null;

            foreach (var kv in horario ?? new Dictionary<DayOfWeek, List<RangoAtencion>>())
            {
                foreach (var r in kv.Value)
                {
                    TimeSpan ini, fin;
                    if (!ParseHora(r.Inicio, out ini) || !ParseHora(r.Fin, out fin))
                        continue;

                    if (!desde.HasValue || ini < desde.Value)
                        desde = ini;
                    if (!hasta.HasValue || fin > hasta.Value)
                        hasta = fin;
                }
            }
        }
    }
}