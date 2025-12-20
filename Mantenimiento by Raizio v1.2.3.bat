@echo off
chcp 850 >nul

:: =============================================
:: LOG GLOBAL - CREA ARCHIVO Y DUPLICA SALIDA
:: =============================================

:: Crear carpeta Logs si no existe
if not exist "%~dp0Logs" mkdir "%~dp0Logs"

:: Crear nombre de archivo con fecha y hora
set "fecha=%date:/=-%"
set "hora=%time::=-%"
set "hora=%hora: =0%"
set "logfile=%~dp0Logs\log_%fecha%_%hora%.txt"

:: Encabezado del log
echo ===== INICIO DEL LOG - %date% %time% ===== > "%logfile%"

:: Activar logging global con PowerShell Transcript
powershell -command "$PSDefaultParameterValues['Out-File:Encoding']='utf8'; Start-Transcript -Path '%logfile%' -Append" >nul 2>&1

:: =============================================
:: Bloque 1 - Encabezado y selección de idioma
:: =============================================
title Mantenimiento de Windows by Raizio v1.2.3
color 0A

echo =============================================
echo   Selecciona el idioma / Select language
echo =============================================
echo 1. Espanol
echo 2. English
echo.
set /p lang=Elige una opcion (Choose an option): 

:: Saltar al menú correcto
if "%lang%"=="1" goto MENU_ES
if "%lang%"=="2" goto MENU_EN

echo Invalid option / Opcion no valida
timeout /t -1 >nul
exit

:MENU_ES
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo =====================================================
    echo   Este script requiere privilegios de ADMINISTRADOR
    echo =====================================================
    echo.
    echo Cierra esta ventana y vuelve a abrir el archivo .bat
    echo haciendo clic derecho y eligiendo "Ejecutar como administrador".
    echo.
    echo Presiona cualquier tecla para salir...
    timeout /t -1 >nul
    exit
)

cls
echo =============================================
echo   MANTENIMIENTO WINDOWS by Raizio - v1.2.3
echo =============================================
echo 1. Reparacion rapida
echo 2. Revisar archivos del sistema
echo 3. Chequeo rapido de imagen
echo 4. Escaneo profundo de imagen
echo 5. Reparar imagen danada
echo 6. Limpieza de componentes
echo 7. Revisar disco duro
echo 8. Liberar espacio en disco
echo 9. Limpiar archivos temporales
echo 10. Optimizar disco
echo 11. Reiniciar configuracion de red
echo 12. Mantenimiento completo
echo 13. Informacion del sistema
echo 14. Salir
set /p opcion=Elige una opcion: 

if "%opcion%"=="1" goto RAPIDA_ES
if "%opcion%"=="2" goto SFC_ES
if "%opcion%"=="3" goto CHECK_ES
if "%opcion%"=="4" goto SCAN_ES
if "%opcion%"=="5" goto RESTORE_ES
if "%opcion%"=="6" goto CLEANUP_ES
if "%opcion%"=="7" goto CHKDSK_ES
if "%opcion%"=="8" goto CLEANMGR_ES
if "%opcion%"=="9" goto TEMP_ES
if "%opcion%"=="10" goto DEFRAG_ES
if "%opcion%"=="11" goto NETRESET_ES
if "%opcion%"=="12" goto ALLFULL_ES
if "%opcion%"=="13" goto SYSINFO_ES
if "%opcion%"=="14" exit
goto MENU_ES

:MENU_EN
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo =====================================================
    echo   This script requires ADMINISTRATOR privileges
    echo =====================================================
    echo.
    echo Close this window and reopen the .bat file
    echo by right-clicking and choosing "Run as administrator".
    echo.
    echo Press any key to exit...
    timeout /t -1 >nul
    exit
)

cls
echo =============================================
echo   WINDOWS MAINTENANCE by Raizio - v1.2.3
echo =============================================
echo 1. Quick repair
echo 2. Check system files
echo 3. Quick image check
echo 4. Deep image scan
echo 5. Repair damaged image
echo 6. Component cleanup
echo 7. Check hard drive
echo 8. Free up disk space
echo 9. Clean temporary files
echo 10. Optimize disk
echo 11. Reset network configuration
echo 12. Full maintenance
echo 13. System information
echo 14. Exit
set /p opcion=Choose an option: 

if "%opcion%"=="1" goto RAPIDA_EN
if "%opcion%"=="2" goto SFC_EN
if "%opcion%"=="3" goto CHECK_EN
if "%opcion%"=="4" goto SCAN_EN
if "%opcion%"=="5" goto RESTORE_EN
if "%opcion%"=="6" goto CLEANUP_EN
if "%opcion%"=="7" goto CHKDSK_EN
if "%opcion%"=="8" goto CLEANMGR_EN
if "%opcion%"=="9" goto TEMP_EN
if "%opcion%"=="10" goto DEFRAG_EN
if "%opcion%"=="11" goto NETRESET_EN
if "%opcion%"=="12" goto ALLFULL_EN
if "%opcion%"=="13" goto SYSINFO_EN
if "%opcion%"=="14" exit
goto MENU_EN

:RAPIDA_ES
echo Ejecutando Reparacion rapida (SFC + DISM basico)...
sfc /scannow
DISM /Online /Cleanup-Image /CheckHealth
DISM /Online /Cleanup-Image /ScanHealth
goto MENU_ES

:RAPIDA_EN
echo Running Quick repair (SFC + basic DISM)...
sfc /scannow
DISM /Online /Cleanup-Image /CheckHealth
DISM /Online /Cleanup-Image /ScanHealth
goto MENU_EN

:SFC_ES
echo Revisando archivos del sistema (SFC)...
sfc /scannow
goto MENU_ES

:SFC_EN
echo Checking system files (SFC)...
sfc /scannow
goto MENU_EN

:CHECK_ES
echo Chequeo rapido de imagen (DISM CheckHealth)...
DISM /Online /Cleanup-Image /CheckHealth
goto MENU_ES

:CHECK_EN
echo Quick image check (DISM CheckHealth)...
DISM /Online /Cleanup-Image /CheckHealth
goto MENU_EN

:SCAN_ES
echo Escaneo profundo de imagen (DISM ScanHealth)...
DISM /Online /Cleanup-Image /ScanHealth
goto MENU_ES

:SCAN_EN
echo Deep image scan (DISM ScanHealth)...
DISM /Online /Cleanup-Image /ScanHealth
goto MENU_EN

:RESTORE_ES
echo Reparando imagen danada (DISM RestoreHealth)...
DISM /Online /Cleanup-Image /RestoreHealth
goto MENU_ES

:RESTORE_EN
echo Repairing damaged image (DISM RestoreHealth)...
DISM /Online /Cleanup-Image /RestoreHealth
goto MENU_EN

:CLEANUP_ES
echo Limpiando componentes (DISM StartComponentCleanup)...
DISM /Online /Cleanup-Image /StartComponentCleanup
goto MENU_ES

:CLEANUP_EN
echo Cleaning components (DISM StartComponentCleanup)...
DISM /Online /Cleanup-Image /StartComponentCleanup
goto MENU_EN

:CHKDSK_ES
echo Revisando disco duro (CHKDSK)...
chkdsk C: /F /R
goto MENU_ES

:CHKDSK_EN
echo Checking hard drive (CHKDSK)...
chkdsk C: /F /R
goto MENU_EN

:CLEANMGR_ES
echo Liberando espacio en disco (Cleanmgr)...
cleanmgr /sagerun:1
goto MENU_ES

:CLEANMGR_EN
echo Freeing up disk space (Cleanmgr)...
cleanmgr /sagerun:1
goto MENU_EN

:TEMP_ES
echo Limpiando archivos temporales (Cleanmgr)...
cleanmgr /sagerun:1
goto MENU_ES

:TEMP_EN
echo Cleaning temporary files (Cleanmgr)...
cleanmgr /sagerun:1
goto MENU_EN

:DEFRAG_ES
echo Optimizando disco (Desfragmentar HDD)...
defrag C: /O
goto MENU_ES

:DEFRAG_EN
echo Optimizing disk (Defragment HDD)...
defrag C: /O
goto MENU_EN

:NETRESET_ES
echo Reiniciando configuracion de red...
ipconfig /flushdns
netsh winsock reset
netsh int ip reset
goto MENU_ES

:NETRESET_EN
echo Resetting network configuration...
ipconfig /flushdns
netsh winsock reset
netsh int ip reset
goto MENU_EN

:ALLFULL_ES
echo Ejecutando MANTENIMIENTO COMPLETO...
sfc /scannow
DISM /Online /Cleanup-Image /CheckHealth
DISM /Online /Cleanup-Image /ScanHealth
DISM /Online /Cleanup-Image /RestoreHealth
DISM /Online /Cleanup-Image /StartComponentCleanup
chkdsk C: /F /R
cleanmgr /sagerun:1
defrag C: /O
ipconfig /flushdns
netsh winsock reset
netsh int ip reset
goto MENU_ES

:ALLFULL_EN
echo Running FULL MAINTENANCE...
sfc /scannow
DISM /Online /Cleanup-Image /CheckHealth
DISM /Online /Cleanup-Image /ScanHealth
DISM /Online /Cleanup-Image /RestoreHealth
DISM /Online /Cleanup-Image /StartComponentCleanup
chkdsk C: /F /R
cleanmgr /sagerun:1
defrag C: /O
ipconfig /flushdns
netsh winsock reset
netsh int ip reset
goto MENU_EN

:SYSINFO_ES
cls
echo ============================================
echo INFORMACION DEL SISTEMA - v1.2.3
echo ============================================

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).Caption"') do set SO=%%a
echo Sistema operativo: %SO%

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).OSArchitecture"') do set Arch=%%a
echo Arquitectura: %Arch%

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).Version"') do set Ver=%%a
echo Version del sistema: %Ver%

echo Nombre del equipo: %COMPUTERNAME%
echo Usuario actual: %USERNAME%

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).Name"') do set CPU=%%a
echo Procesador: %CPU%

for /f %%i in ('powershell -NoProfile -Command "(Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB"') do set RAMGB=%%i
echo Memoria RAM total: %RAMGB% GB

:: FIX robusto: evitar crash y notación científica
set FreeGB=
for /f "usebackq delims=" %%i in (`powershell -NoProfile -Command "[math]::Round((Get-CimInstance Win32_LogicalDisk -Filter 'DeviceID=''C:''').FreeSpace / 1GB)"`) do set FreeGB=%%i
if "%FreeGB%"=="" set FreeGB=0
echo Espacio libre en disco C: %FreeGB% GB

echo.
echo Presiona cualquier tecla para volver al menu en Espanol...
timeout /t -1 >nul
goto MENU_ES


:SYSINFO_EN
cls
echo ============================================
echo SYSTEM INFORMATION - v1.2.3
echo ============================================

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).Caption"') do set SO=%%a
echo Operating System: %SO%

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).OSArchitecture"') do set Arch=%%a
echo Architecture: %Arch%

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_OperatingSystem).Version"') do set Ver=%%a
echo System Version: %Ver%

echo Computer Name: %COMPUTERNAME%
echo Current User: %USERNAME%

for /f "delims=" %%a in ('powershell -NoProfile -Command "(Get-CimInstance Win32_Processor).Name"') do set CPU=%%a
echo Processor: %CPU%

for /f %%i in ('powershell -NoProfile -Command "(Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB"') do set RAMGB=%%i
echo Total RAM: %RAMGB% GB

:: FIX robust: avoid crash and scientific notation
set FreeGB=
for /f "usebackq delims=" %%i in (`powershell -NoProfile -Command "[math]::Round((Get-CimInstance Win32_LogicalDisk -Filter 'DeviceID=''C:''').FreeSpace / 1GB)"`) do set FreeGB=%%i
if "%FreeGB%"=="" set FreeGB=0
echo Free space on disk C: %FreeGB% GB

echo.
echo Press any key to return to the menu in English...
timeout /t -1 >nul
goto MENU_EN


:: =============================================
:: CIERRE DEL LOG GLOBAL
:: =============================================
powershell -command "Stop-Transcript" >nul 2>&1
echo ===== FIN DEL LOG - %date% %time% ===== >> "%logfile%"