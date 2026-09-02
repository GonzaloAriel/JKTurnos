# CONTRATO FUNCIONAL - VIDRIERA PUBLICA DEL TENANT (MODULO M-13)

FORMATO: 1.0
Estado: CONTRATO (documento de definicion. Sin codigo.)
Fecha: 2026-09-02
Reemplaza a: M-08 (PORTAL DE RESERVAS ONLINE) - DESCARTADO por cambio de vision.

## 1. VISION DE UNA LINEA

La Vidriera Publica es la vista publica e informativa de un negocio que consulta en
tiempo real los datos operativos del tenant (servicios, precios, disponibilidad,
horarios, ofertas) para anuncios en monitores, capturas de pantalla y landing page,
PERO que no realiza ninguna accion interna del sistema.

## 2. QUE ES

- Una pagina publica por tenant (URL propia).
- Muestra datos REALES del sistema: servicios, precios, duracion, recursos,
  disponibilidad de turnos, horarios, identidad del negocio.
- Permite al admin publicar/ocultar informacion y mensajes/ofertas desde el panel.
- Se actualiza sola (refresh periodico) reflejando cambios de disponibilidad.
- Disenada para lectura a distancia: monitores, TV, capturas de pantalla.
- Totalmente aislada del backoffice (sin login, sin navegacion interna).

## 3. QUE NO ES (FUERA DE ALCANCE - version 1)

Las siguientes quedan EXPLICITAMENTE FUERA:
- Reservas / alta de turno desde la vidriera.
- Login de clientes / cuentas publicas.
- Pagos / carrito / checkout.
- CMS completo / constructor visual / drag & drop.
- Multiples paginas (es UNA vista).
- WebSockets / SignalR (solo refresh periodico al inicio).
- Estadisticas complejas / metricas de visitas.
- Campanas publicitarias / multiples banners.
- Subdominios como URL primaria (evolucion posterior, mismo motor).
- API JSON publica de servicios/agenda (version 1 usa vista + meta refresh).

## 4. INFORMACION QUE PUEDE MOSTRAR

Identidad:
- Nombre comercial (ConfiguracionTenant.NombreComercial)
- Logo (LogoUrl / AvatarUrl)
- Telefono, email, direccion (ConfiguracionTenant)
- Mensaje destacado / anuncio (config de la vidriera)

Oferta:
- Titulo/slogan principal (config)
- Descripcion (config)

Servicios (los que el tenant decida mostrar):
- Nombre, descripcion, precio, duracion
- Recurso(s) que lo prestan
- Disponibilidad real (proximos slots libres) si el tenant lo habilita

Informacion operativa:
- Horarios de atencion / jornada
- Disponibilidad del dia por servicio/recurso
- Proximos turnos disponibles

## 5. INFORMACION QUE NUNCA DEBE EXPONER

- Datos de clientes (nombres, telefonos, emails, historial).
- Observaciones / notas internas de turnos.
- IDs internos de turnos, clientes, usuarios, recursos (solo lo necesario para la vista).
- Credenciales, claves, tokens, datos SMTP.
- Configuracion interna (plantillas WhatsApp/email, recordatorios, SMTP).
- Estados economicos ni datos de facturacion/suscripcion.
- Datos de otros tenants (aislamiento estricto - ver seccion 9).
- Cualquier accion: no hay formularios, POST, ni endpoints de escritura publicos.

## 6. QUE PUEDE MODIFICAR EL TENANT (desde el panel, NO desde la vista publica)

- Publicar / despublicar la vidriera.
- Titulo / slogan.
- Descripcion del negocio.
- Mensaje destacado / anuncio del dia.
- Oferta destacada (titulo + texto).
- Mostrar/ocultar: servicios, precios, disponibilidad, proximos turnos, ofertas.
- Orden / seleccion de que servicios aparecen.
- Activar/desactivar el modo monitor.

## 7. QUE DATOS VIENEN DEL SISTEMA vs MANUAL

Vienen del sistema (siempre actuales, no se duplican):
- Servicios, precios, duracion (Servicio)
- Recursos que prestan cada servicio (RecursoServicio)
- Disponibilidad / slots libres (calculado desde Turno + EstadoTurno)
- Identidad del negocio (ConfiguracionTenant)
- Horarios/jornada (pendiente: depende de M-01 horarios configurables)

Son manuales (configuracion propia de la vidriera):
- Titulo / slogan
- Descripcion / mensaje destacado
- Oferta destacada
- Flags de visibilidad

## 8. COMO SE IDENTIFICA UN TENANT

Version 1: SLUG en la URL  ->  /vidriera/{slug}
- Se agrega un campo SlugVidriera (unico) al Tenant o a la config de la vidriera.
- El slug es un identificador publico legible y no-guessable (ej: "demo", "gonzo-beauty").
- No se depende de claims (la vista es anonima) ni de subdominios locales.

Evolucion posterior (fuera de alcance v1):
- Subdominio (demo.jkturnos.com) resolviendo al mismo motor.

## 9. REGLAS DE PUBLICACION / SEGURIDAD

- La vidriera SOLO se muestra si:
  * el tenant existe y esta IsActive = true, y
  * la vidriera esta Habilitada = true.
- Si el tenant esta inactivo, suspendido, vencido, o la vidriera despublicada:
  * respuesta 404 o pagina de "no disponible" (NO exponer datos).
- Aislamiento estricto: cualquier consulta se filtra SIEMPRE por TenantId resuelto.
- No hay ni una sola accion de escritura publica (GET read-only).
- Headers de cache adecuados para no cachear datos sensibles ni stale en monitores.
- Sin IDs en la URL que permitan recorrer tenants: solo el slug.
- El slug es opaco (no secuencial) para evitar enumeracion.

## 10. CRITERIOS DE ACEPTACION (version 1)

1. Un admin publica su vidriera y accede por /vidriera/{slug} sin login.
2. La vista muestra identidad, servicios, precios y (si habilita) disponibilidad real.
3. Al asignar/liberar un turno desde el panel, la vidriera refleja el cambio tras el
   refresh periodico (max ~30 s).
4. El admin puede publicar/despublicar, cambiar mensaje y oferta, y mostrar/ocultar
   secciones desde el panel.
5. Un visitante NO puede navegar a ningun modulo interno, iniciar sesion, ni
   modificar datos. Manipular URLs no otorga acceso.
6. Un tenant inactivo o vidriera despublicada NO expone informacion.
7. La vista es legible en monitor/TV y queda bien en captura de pantalla.
8. MSBuild Debug 0 errores y MvcBuildViews OK.

## 11. NOTA SOBRE EL DESCARTE DE M-08

M-08 (Portal de Reservas Online + WhatsApp) queda DESCARTADO y eliminado del
codigo. La Vidriera NO lo reemplaza funcionalmente: no reserva, solo informa.
Se documenta como cambio de vision del producto (ver MEMORIA/ESTADO).
