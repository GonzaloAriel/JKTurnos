Resumen del día — Design System JKTurnos
Componentes implementados y funcionando
- _CrudPage.cshtml — wrapper Index (PageHeader + CrudToolbar + content slot)
- _CrudToolbar.cshtml — barra de búsqueda + filtros
- _TableActions.cshtml — acciones por fila (editar/eliminar)
- _FormLayout.cshtml — wrapper Create/Edit (PageHeader + card + form + anti-forgery + actions)
- _FormActions.cshtml — botones submit/cancel
Vistas migradas (6)
- Clientes: Index, Create, Edit
- Servicios: Index, Create, Edit
Bugs corregidos
1. Comentarios @* @* anidados en _CrudPage y _FormLayout — Razor v3 no soporta anidación, el *@ interno cerraba el bloque externo exponiendo el ejemplo como código real y dejando un @ suelto
2. @if dentro de @using { } — el body de @using está en contexto código, @if es incorrecto (debe ser solo if)
3. Propiedad EsDisponible no existe en Servicio.cs — eliminada de Create y Edit
Patrón adoptado
- @helper para generar contenido que se pasa vía ViewBag a los componentes (evita @<text> y Func<>)
- Componentes reciben ViewBag.PageContent / ViewBag.FormContent sin casteo ni tipos genéricos
- Vistas mantienen el @model y lógica condicional en @{}, helpers debajo
Pendiente / Próximos pasos
- Agregar los 6–8 módulos restantes (Turnos, usuarios, etc.) observando patrones antes de abstraer más
- Evaluar si _CrudPage y _FormLayout se consolidan o se mantienen separados
- _FormField y CrudIndexViewModel archivados — se reconsideran cuando haya más módulos
- Documentar el patrón en AGENTS.md para mantener consistencia