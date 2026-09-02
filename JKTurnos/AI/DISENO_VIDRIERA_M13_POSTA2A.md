# DISEÑO - VIDRIERA PUBLICA DEL TENANT (M-13) - POSTA 2A

FORMATO: 1.0
Estado: DISEÑO (sin codigo, sin DB, sin vistas. Pendiente aprobacion TEAM LEAD).
Fecha: 2026-09-02
Base: CONTRATO_VIDRIERA_M13.md (Posta 0) + AUDITORIA_VIDRIERA_M13.md (Posta 1)

Objetivo: definir el modelo minimo de configuracion y separarlo de los datos que
ya vienen del sistema. Su unico resultado es este documento (no modifica nada).

## 1. PRINCIPIO RECTOR

La Vidriera NO crea un segundo sistema de datos. Consulta las entidades
existentes y guarda SOLO la configuracion manual minima que necesita el tenant
para controlar digitalmente su vidriera. Cualquier dato que ya existe en el
sistema se sigue leyendo de ahi (fuente de verdad unica, sin duplicacion).

## 2. DATOS QUE VIENEN DEL SISTEMA (NO se duplican)

| Necesidad de la vidriera | Fuente existente | Nota |
|---|---|---|
| Nombre/identidad del negocio | ConfiguracionTenant.NombreComercial | existe |
| Logo / avatar | ConfiguracionTenant.LogoUrl / AvatarUrl | existe |
| Telefono / email / direccion | ConfiguracionTenant.Telefono/Email/Direccion | existe |
| Servicios publicados | Servicio (TenantId) | existe |
| Precio y duracion | Servicio.Precio / DuracionMinutos | existe |
| Recursos que prestan un servicio | RecursoServicio + Recurso | existe |
| Disponibilidad / slots libres | Turno + EstadoTurno (+ Recurso) | CALCULADO (nuevo metodo) |
| Horarios de atencion | jornada fija 08:00-19:30 (D-012) | limitacion v1, hasta M-01 |

NO se crean VidrieraServicio, VidrieraTurno, VidrieraPrecio ni ninguna tabla
que duplique datos del sistema. La unica decision de "que servicios mostrar" es
por exclusión desde la config (ver seccion 6), y aun asi reusa la entidad Servicio.

## 3. DECISION: DONDE VIVE LA CONFIGURACION (y porque)

Opciones evaluadas:

A) Campos sueltos sobre ConfiguracionTenant
   - Pro: sin tabla nueva; 1 query para identidad + vidriera.
   - Contra: mezcla config de negocio/notificaciones con config de publicidad;
     agrega ~10 logs de [StringLength] a una clase ya extensa; acopla conceptos.

B) Entidad nueva VidrieraPublica (1:1 con Tenant, via TenantId)
   - Pro: separacion limpia de responsabilidad; la clase es chica y crece sola;
     aislamiento claro (filtra por TenantId como ITenantEntity, cosistente con
     el resto del modelo operativo); el slug publico vive aca.
   - Contra: 1 query + join mas.

C) Campos en Tenant
   - Pro: sin join.
   - Contra: Tenant es entidad de plataforma (D-022) NO es ITenantEntity; meter
     config de publicidad ahi romperia la regla "Tenant es de plataforma".

DECISION PROPUESTA: OPCION B - entidad nueva VidrieraPublica, 1:1 con Tenant.
- Respeta el patron ITenantEntity del resto de entidades operativas.
- Mantiene Tenant como entidad de plataforma (D-022).
- Mantiene ConfiguracionTenant como identidad + notificaciones.
- El slug publico vive aca (es config de la vidriera, no de plataforma).

Alternativa B' (si el TEAM LEAD prefiere minimo absoluto): quitar la entidad
nueva y poner los campos sobre ViabilidadTenant via campos planos. NO
recomendada por el acoplamiento, pero viable. La decision se deja explicita.

## 4. MODELO FINAL PROPUESTO

### Nueva entidad: VidrieraPublica

```
public class VidrieraPublica : BaseEntity, ITenantEntity
{
    public int TenantId { get; set; }                // FK a Tenant, 1:1

    // Identificacion publica
    public string Slug { get; set; }                 // unico, lower, sin espacios

    // Interruptor de publicacion
    public bool Habilitada { get; set; }             // default false

    // Contenido manual
    public string Titulo { get; set; }               // [StringLength(150)]
    public string Descripcion { get; set; }          // [StringLength(500)]
    public string MensajePrincipal { get; set; }     // [StringLength(500)] anuncio/mensaje
    public string OfertaTitulo { get; set; }         // [StringLength(150)]
    public string OfertaDescripcion { get; set; }    // [StringLength(500)]

    // Flags de visibilidad de secciones
    public bool MostrarServicios { get; set; }       // default true
    public bool MostrarPrecios { get; set; }         // default true
    public bool MostrarDisponibilidad { get; set; }  // default false (requiere consulta)
    public bool MostrarOfertas { get; set; }         // default true
    public bool MostrarMensaje { get; set; }         // default true

    [ForeignKey("TenantId")]
    public virtual Tenant Tenant { get; set; }
}
```

### Relaciones y TenantId

- 1:1 con Tenant. `VidrieraPublica.TenantId -> Tenant.Id`.
- `TenantId` NUNCA se expone ni viaja en la URL publica (se resuelve desde Slug).
- Es ITenantEntity, igual que Servicio/Turno/etc., para que BaseRepository lo
  filtre por tenant cuando el admin edita (via claims).

### Campos manuales vs consultas en tiempo real

MANUALES (guardados en VidrieraPublica): Slug, Habilitada, Titulo,
Descripcion, MensajePrincipal, OfertaTitulo, OfertaDescripcion, y los 5
Mostrar*.

CONSULTAS EN TIEMPO REAL (NO guardados): nombre/logo/contacto
(ConfiguracionTenant), servicios/precios/duracion (Servicio), recursos por
servicio (RecursoServicio), disponibilidad (calculada desde Turno+EstadoTurno).

## 5. DECISION DEL SLUG

- El Slug pertenece a VidrieraPublica (config de la vidriera), NO a Tenant.
- URL v1: /vidriera/{slug}
- El Slug es un identificador publico legible, opcional del negocio, opaco y
  NO secuencial (no es un ID) -> evita enumeracion de tenants.
- Resuelve inequivocamente a TenantId via VidrieraService.GetBySlug(slug).
- Normalizacion: lower + trim (mismo estilo que Subdominio en TenantService).
- Unicidad GLOBAL (no por-tenant, porque el slug es la llave publica de URL).

## 6. SELECCION DE SERVICIOS MOSTRADOS (sin tabla extra)

Version 1 NO guarda "que servicios mostrar" como lista. Reutiliza la entidad
Servicio y aplica una regla simple de exclusion por config:

- Si el tenant quiere ocultar un servicio puntual, puede marcarlo como
  serviciono-publico... PERO eso NO existe hoy.

DECISION (v1, minima): la vidriera muestra TODOS los servicios activos del
tenant (Servicio where IsActive). NO hay lista de exclusion en v1.
- Si aparece una necesidad real de ocultar servicios puntuales, se agrega una
  coleccion VidrieraServicio MOSTRADA/OCULTADA en Posta posterior (fuera de
  alcance ahora). No se anticipa.

Esto mantiene el modelo minimo y evita VidrieraServicio sin necesidad real
(alineado con Posta 1 y con tu pedido de "no crear VidrieraServicio").

## 7. INDICES Y CONSTRAINTS NECESARIOS

Tabla VidrieraPublicas:
- PK: Id (convencion BaseEntity).
- FK: TenantId -> Tenant.Id. UNICO (1:1). El TenantId debe ser unico porque
  cada tenant tiene UNA vidriera.
- UNICO global en Slug (la llave publica de URL). No por-tenant.
- TenantId filtrable por BaseRepository (indice de FK, igual que el resto).
- Habilitada: sin indice (volumen bajisimo).

En la migracion:
- CreateTable de VidrieraPublicas.
- UNIQUE en Slug.
- UNIQUE idx en TenantId (1:1).
- Sin cambios en Tablas existentes (identidad y servicios se reusan).

## 8. SEMANTICA DE PUBLICACION (alineada con Posta 0)

- VidrieraPublica SIN fila / Habilitada=false  -> 404
- Habilitada=true + tenant sin datos de config  -> 404 (no expone)
- Habilitada=true + tenant IsActive=false        -> 404
- Habilitada=true + tenant activo               -> PUBLICA

El controlador publico resuelve por Slug, chequea tenant.IsActive y
vidriera.Habilitada, y solo entonces arma el ViewModel.

## 9. LO QUE SE AGREGA AL SISTEMA (resumen de diffs)

NUEVAS Clases/Archivos:
1. Models/Entities/VidrieraPublica.cs  (entidad nueva)
2. Migracion AddVidrieraPublica (tabla + indices/unique)
3. Services/VidrieraService.cs (lectura publica + GetAvailableSlots en Posta 4)
4. Controllers/VidrieraController.cs (publico [AllowAnonymous], read-only)
5. Views/Vidriera/Index.cshtml + layout publico
6. Seccion Gestion -> Vidriera (edicion de config) en Posta 5
7. Models/ViewModels/VidrieraViewModel.cs + VidrieraConfigViewModel.cs

CAMBIOS a archivos existentes:
- JKContext.cs: +DbSet<VidrieraPublica> (+ config FK 1:1 si hace falta)
- JKTurnos.csproj: +Compile entries
- RouteConfig.cs: +ruta nombrada "Vidriera"  /vidriera/{slug}

SIN CAMBIOS a: Tenant.cs, ConfiguracionTenant.cs, Servicio.cs, Turno.cs,
Recurso.cs, EstadoTurno.cs, RecursoServicio.cs (todos se reusan tal cual).

## 10. PENDIENTE / A DEFINIR ANTES DE IMPLEMENTAR

- Confirmar OPCION B (entidad VidrieraPublica) vs B' (campos en
  ConfiguracionTenant).
- Confirmar que el slug viva en VidrieraPublica y sea unico global.
- Confirmar el patrón de seleccion de servicios (todos activos) sin tabla de
  exclusion en v1.
- Confirmar nombres de campos (nomenclatura del proyecto: PascalCase, sin
  acentos, comentarios en espanol con prefijo D-xx para decisiones).
- Definir numeracion de decision (proxima D disponible) para registrar la
  decision del modelo en 02_DECISIONES.md.

## 11. DECISIONES NO TOMADAS (fuera de Posta 2A)

- Algoritmo exacto de GetAvailableSlots (Posta 4).
- Forma del ViewModel publico (Posta 3).
- Seccion/UI del panel de gestion (Posta 5).
- Refresh tecnico (meta vs fetch) (Posta 6).
- Modo monitor (Posta 7).
- Seguridad/cache (Posta 8).
