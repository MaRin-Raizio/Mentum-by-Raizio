Mentum 2.4.0
------------

El nombre "Mentum" surge de la raíz latina del sufijo "-mentum", que significa acción,
efecto o proceso. Lo encontramos en palabras como "mantenimiento", que proviene de
"manus" (mano) y "tenere" (sostener), acompañado de "-mentum" como el acto de mantener.
Mentum, entonces, evoca frescura, renovación, el proceso de sostener y dar vida a lo
que parecía desgastado. Suena a menta, a algo fresco, a lo que está "amentado", y esa
es precisamente la esencia: mantener las cosas vivas, claras y funcionando.

Este software es mi regalo para el mundo. Nació en un tiempo en el que mi carrera aún
no podía ejercerse, cuando los recursos eran escasos y la incertidumbre era grande.
Mentum fue mi escape, mi refugio, mi manera de transformar la frustración en creación.
Cada línea de código es testimonio de esa lucha, de esa esperanza, de esa voluntad de
dejar una huella. Es mi forma de decir: aquí estoy, y aunque no tenga todo, puedo dar
algo que quizá ilumine a otros.

Mentum no es solo un programa: es un compañero que busca aportar claridad y confianza
en cada ejecución. Su propósito es ayudarte a mantener tu sistema transparente,
organizado y confiable, con un toque humano y poético.

Funciones principales:
----------------------
- Reparación del sistema: SFC, DISM CheckHealth, ScanHealth y RestoreHealth con un clic.
- Reparación de red: reinicio completo de IP, DNS, Winsock y TCP/IP.
- Disco y rendimiento: revisión con CHKDSK y optimización/desfragmentación.
- Limpieza de temporales: eliminación de archivos temporales con cleanmgr.
- Limpieza profunda: limpieza completa del sistema incluyendo caché de Windows Update,
  carpetas Temp, Prefetch, papelera de reciclaje y caché de miniaturas.
- Seguridad y privacidad: limpieza de historial de actividad, reparación de accesos
  directos rotos, administrador de programas de inicio con opción de habilitar o
  deshabilitar cada uno de forma reversible, y verificación del estado de Windows
  Defender y del Firewall, con reconocimiento de antivirus de terceros.
- Reporte de batería: genera con un clic el informe detallado de Windows y lo abre en
  tu navegador (para equipos portátiles).
- Mantenimiento completo: secuencia automática de SFC, DISM, CHKDSK, optimización,
  limpieza de historial y reparación de accesos directos.
- Información del sistema: datos en tiempo real de CPU, RAM, actividad de red, disco
  y hardware, con prueba de velocidad de descarga bajo demanda.
- Opciones avanzadas: más de 60 comandos individuales organizados por categoría
  (reparación, disco, limpieza, red, sistema y arranque, energía y hardware), con
  buscador en vivo, para quien ya sabe lo que necesita. Las herramientas que abren su
  propia ventana no bloquean Mentum.
- Registro de acciones: log detallado con timestamps de todo lo ejecutado,
  exportable a .txt con un clic.
- Indicador de tiempo: muestra el tiempo transcurrido durante cada operación.
- Cancelación de procesos: posibilidad de detener cualquier operación en curso.
- Confirmaciones de seguridad: aviso antes de ejecutar operaciones destructivas.
- Notificaciones al completar: sonido, restauración de ventana y flash en la barra
  de tareas al terminar cualquier operación, incluso si Mentum está minimizado.
- Tema claro y oscuro: alterna entre ambos con un solo clic, con la preferencia
  guardada entre sesiones.
- Historial de versiones: consulta qué cambió en cada entrega desde la propia
  aplicación, con el botón de la barra de título.
- Branding integrado: acceso directo a GitHub y Ko-fi para comunidad y soporte.

Requisitos del sistema:
-----------------------
- Windows 10 o superior (64 bits)
- .NET 8.0 o superior
- Permisos de administrador (requeridos para la mayoría de las operaciones)

Novedades en 2.3.0:
-------------------
- Nueva sección de Seguridad y Privacidad con limpieza de historial, reparación de
  accesos directos rotos y visor de programas de inicio.
- Nuevo tema claro, con toggle de sol/luna en la barra de título y preferencia guardada.
- Nueva tarjeta de velocidad de red en tiempo real en Información del sistema.
- Corregido un error que impedía vaciar la papelera de reciclaje correctamente
  durante la limpieza profunda.
- Corregidos varios problemas de distribución visual en la ventana principal y en
  Información del sistema.

Novedades en 2.3.1 (parche de correcciones):
---------------------------------------------
- Corregido un error que hacía crashear Mentum al reparar accesos directos rotos
  en carpetas del sistema con permisos restringidos.
- Corregida la actividad de red, que no funcionaba correctamente en algunas
  instalaciones de Windows 10.
- El administrador de programas de inicio ahora permite habilitar y deshabilitar
  cada entrada de forma reversible, en lugar de solo mostrarlas en el registro.
- CHKDSK ahora programa correctamente la comprobación en el próximo reinicio en
  lugar de fallar en silencio por falta de confirmación.
- Nuevo aviso en el registro de acciones cuando un cambio requiere reiniciar el
  equipo para aplicarse por completo.
- La prueba de velocidad ahora verifica la conexión a internet en unos segundos
  antes de iniciar, evitando esperas largas innecesarias cuando no hay conexión.
- Renombrada la tarjeta "Velocidad de red" a "Actividad de red" para no
  confundirla con el nuevo botón de prueba de velocidad bajo demanda.
- Tema claro ajustado a un tono crema más suave, menos intenso que la versión
  anterior.

Novedades en 2.4.0 — El Yunque:
-------------------------------
- Herramientas avanzadas renovada por completo: más de 60 comandos individuales
  organizados por categoría, cada uno con una breve descripción de lo que hace.
- Nuevo buscador en vivo en Herramientas avanzadas: filtra por comando o por
  descripción y oculta las categorías sin resultados.
- Los comandos ya no congelan Mentum mientras se ejecutan. Las herramientas que abren
  su propia ventana (winver, msinfo32, dxdiag, eventvwr, entre otras) se lanzan sin
  esperar a que las cierres.
- Nueva advertencia de modo avanzado, que se muestra una sola vez por sesión al abrir
  Herramientas avanzadas por primera vez.
- Nuevo botón Verificar Defender y Firewall: muestra el estado del antivirus, la
  protección en tiempo real y los tres perfiles del Firewall de Windows.
- La verificación reconoce cuándo hay un antivirus de terceros activo (en ese caso
  Defender está en pausa a propósito) y distingue los restos de uno ya desinstalado,
  para no mostrar falsas alarmas.
- Nuevo botón Reporte de batería: genera el informe detallado de Windows y lo abre en
  tu navegador. Si el equipo no tiene batería, lo avisa en el registro de acciones.
- Nuevo historial de versiones dentro de la aplicación, accesible desde la barra de
  título.
- La sección de Seguridad y Privacidad pasa a tener dos filas de botones.

Donaciones voluntarias:
-----------------------
Mentum es y será gratuito. Sin embargo, si este proyecto te inspira, te ayuda o
simplemente deseas apoyarlo, puedes hacerlo a través de Ko-fi. No es obligación ni
expectativa: es solo una invitación abierta para quienes deseen contribuir a que
Mentum siga creciendo. Tu apoyo, sea grande o pequeño, es una señal de que este regalo
encuentra eco en otros corazones.

Gracias por instalarlo y ser parte de este viaje. Gracias por creer en mí.

- MaRin Raizio
