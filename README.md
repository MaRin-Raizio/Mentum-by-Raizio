\# 🛠️ Mantenimiento de Windows by Raizio



\*\*Versión actual:\*\* v1.1.0 (corrected)  

\*\*Autor:\*\* Raizio



---



\## 📖 Descripción general



\*Mantenimiento de Windows by Raizio\* es un script en \*\*Batch (.bat)\*\* diseñado para facilitar tareas de diagnóstico y optimización en sistemas Windows.  

Su objetivo es ofrecer una herramienta sencilla y confiable para mantener el sistema en buen estado, con un menú claro y opciones automatizadas que ahorran tiempo al usuario.



---



\## ⚙️ Funciones principales



\- 🔧 \*\*Reparación rápida\*\*: combina SFC y DISM para detectar y corregir errores básicos.  

\- 📝 \*\*Verificación de archivos del sistema\*\* con `sfc /scannow`.  

\- 🛡️ \*\*Chequeo y reparación de imagen de Windows\*\* con DISM (`CheckHealth`, `ScanHealth`, `RestoreHealth`).  

\- 🧹 \*\*Limpieza de componentes y archivos temporales\*\*.  

\- 💾 \*\*Optimización de disco\*\* (desfragmentación en HDD).  

\- 🌐 \*\*Reinicio de configuración de red\*\* (DNS, Winsock, IP).  

\- 🚀 \*\*Mantenimiento completo\*\*: ejecuta todos los pasos en una sola rutina.  

\- 🔍 \*\*Información del sistema\*\*: muestra versión de Windows, nombre del equipo y espacio libre en disco.  

\- 📊 \*\*Log detallado\*\*: cada paso se registra en `mantenimiento\_log.txt` indicando \*\*ÉXITO\*\* o \*\*ERROR\*\*.  

\- 📋 \*\*Resumen en pantalla\*\*: al finalizar el mantenimiento completo, se muestra el log directamente en la consola.



---



\## 📥 Instalación y uso



1\. Descarga el archivo `.bat` desde la sección \*\*Releases\*\*.  

2\. Guarda el archivo en tu PC.  

3\. Haz clic derecho → \*Ejecutar como administrador\*.  

4\. Selecciona la opción deseada en el menú interactivo.  



> ⚠️ Nota: algunas funciones como `CHKDSK` pueden requerir reinicio del sistema.



---



\## 🗂️ Releases



\- \*\*v1.1.0 (corrected)\*\*  

&nbsp; - Log automático con estado EXITO/ERROR.  

&nbsp; - Resumen en pantalla al finalizar mantenimiento completo.  

&nbsp; - Nueva opción de información del sistema.  

&nbsp; - Mejoras de claridad en menú.



\- \*\*v1.0.2\*\*  

&nbsp; - Versión inicial publicada en GitHub.  

&nbsp; - Funciones básicas de mantenimiento (SFC, DISM, limpieza, optimización, red).



---



\## 🤝 Contribuciones



Este proyecto está abierto a mejoras.  

Si tienes sugerencias, abre un \*Issue\* o envía un \*Pull Request\* en GitHub.



---



\## 📜 Licencia



Este proyecto se distribuye bajo la licencia MIT.  

Puedes usarlo, modificarlo y compartirlo libremente, siempre dando crédito al autor.



