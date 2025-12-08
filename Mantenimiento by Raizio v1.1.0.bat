@echo off
title Mantenimiento de Windows by Raizio v1.1.0
color 0A
setlocal enabledelayedexpansion

:: Definir archivo de log en la misma carpeta del script
set "log=%~dp0mantenimiento_log.txt"

:MENU
cls
echo =============================================
echo        MANTENIMIENTO WINDOWS by Raizio
echo =============================================
echo.
echo 1. Reparacion rapida (SFC + DISM basico)
echo 2. Revisar archivos del sistema (SFC)
echo 3. Chequeo rapido de imagen (DISM CheckHealth)
echo 4. Escaneo profundo de imagen (DISM ScanHealth)
echo 5. Reparar imagen danada (DISM RestoreHealth)
echo 6. Limpieza de componentes (DISM StartComponentCleanup)
echo 7. Revisar disco duro (CHKDSK)
echo 8. Liberar espacio en disco
echo 9. Limpiar archivos temporales (Cleanmgr)
echo 10. Optimizar disco (Desfragmentar HDD)
echo 11. Reiniciar configuracion de red
echo 12. Mantenimiento completo (TODOS LOS PASOS)
echo 13. Informacion del sistema
echo 14. Salir
echo.
set /p opcion=Elige una opcion: 

if "%opcion%"=="1" goto RAPIDA
if "%opcion%"=="2" goto SFC
if "%opcion%"=="3" goto CHECK
if "%opcion%"=="4" goto SCAN
if "%opcion%"=="5" goto RESTORE
if "%opcion%"=="6" goto CLEANUP
if "%opcion%"=="7" goto CHKDSK
if "%opcion%"=="8" goto CLEANMGR
if "%opcion%"=="9" goto TEMP
if "%opcion%"=="10" goto DEFRAG
if "%opcion%"=="11" goto NETRESET
if "%opcion%"=="12" goto ALLFULL
if "%opcion%"=="13" goto SYSINFO
if "%opcion%"=="14" exit
goto MENU

:ProgressBar
set "bar="
for /l %%i in (1,1,30) do (
    set "bar=!bar!#"
    cls
    echo Progreso: [!bar!]
    timeout /nobreak /t 1 >nul
)
goto :eof

:LogResult
if %errorlevel%==0 (
    echo [%date% %time%] %1 - EXITO >> "%log%"
) else (
    echo [%date% %time%] %1 - ERROR (codigo %errorlevel%) >> "%log%"
)
goto :eof

:ReturnMenu
echo.
echo === Paso finalizado correctamente ===
for /l %%i in (5,-1,1) do (
    echo Regresando al menu principal en %%i segundos...
    timeout /nobreak /t 1 >nul
)
goto MENU

:RAPIDA
echo Ejecutando Reparacion rapida (SFC + DISM basico)...
sfc /scannow
call :LogResult "SFC"
DISM /Online /Cleanup-Image /CheckHealth
call :LogResult "DISM CheckHealth"
DISM /Online /Cleanup-Image /ScanHealth
call :LogResult "DISM ScanHealth"
goto ReturnMenu

:SFC
echo Revisando archivos del sistema...
sfc /scannow
call :LogResult "SFC"
goto ReturnMenu

:CHECK
echo Chequeo rapido de imagen...
DISM /Online /Cleanup-Image /CheckHealth
call :LogResult "DISM CheckHealth"
goto ReturnMenu

:SCAN
echo Escaneo profundo de imagen...
DISM /Online /Cleanup-Image /ScanHealth
call :LogResult "DISM ScanHealth"
goto ReturnMenu

:RESTORE
echo Reparando imagen danada...
DISM /Online /Cleanup-Image /RestoreHealth
call :LogResult "DISM RestoreHealth"
goto ReturnMenu

:CLEANUP
echo Limpiando componentes...
DISM /Online /Cleanup-Image /StartComponentCleanup
call :LogResult "DISM Cleanup"
goto ReturnMenu

:CHKDSK
echo Revisando disco duro (puede requerir reinicio)...
chkdsk C: /F /R
call :LogResult "CHKDSK"
goto ReturnMenu

:CLEANMGR
echo Liberando espacio en disco con herramienta oficial de Windows...
cleanmgr /sagerun:1
call :LogResult "Cleanmgr"
goto ReturnMenu

:TEMP
echo Limpiando archivos temporales con herramienta oficial de Windows...
cleanmgr /sagerun:1
call :LogResult "Cleanmgr temporales"
goto ReturnMenu

:DEFRAG
echo Optimizando disco (solo HDD)...
defrag C: /O
call :LogResult "Defrag"
goto ReturnMenu

:NETRESET
echo Reiniciando configuracion de red...
ipconfig /flushdns
call :LogResult "FlushDNS"
netsh winsock reset
call :LogResult "Winsock Reset"
netsh int ip reset
call :LogResult "IP Reset"
goto ReturnMenu

:ALLFULL
echo ============================================
echo Ejecutando MANTENIMIENTO COMPLETO (TODOS LOS PASOS)
echo ============================================

echo Paso 1: Revisar archivos del sistema
call :ProgressBar
sfc /scannow
call :LogResult "SFC"

echo Paso 2: Chequeo rapido de imagen
call :ProgressBar
DISM /Online /Cleanup-Image /CheckHealth
call :LogResult "DISM CheckHealth"

echo Paso 3: Escaneo profundo de imagen
call :ProgressBar
DISM /Online /Cleanup-Image /ScanHealth
call :LogResult "DISM ScanHealth"

echo Paso 4: Reparar imagen danada
call :ProgressBar
DISM /Online /Cleanup-Image /RestoreHealth
call :LogResult "DISM RestoreHealth"

echo Paso 5: Limpieza de componentes
call :ProgressBar
DISM /Online /Cleanup-Image /StartComponentCleanup
call :LogResult "DISM Cleanup"

echo Paso 6: Revisar disco duro
call :ProgressBar
chkdsk C: /F /R
call :LogResult "CHKDSK"

echo Paso 7: Liberar espacio en disco
call :ProgressBar
cleanmgr /sagerun:1
call :LogResult "Cleanmgr"

echo Paso 8: Limpiar archivos temporales
call :ProgressBar
cleanmgr /sagerun:1
call :LogResult "Cleanmgr temporales"

echo Paso 9: Optimizar disco
call :ProgressBar
defrag C: /O
call :LogResult "Defrag"

echo Paso 10: Reiniciar configuracion de red
call :ProgressBar
ipconfig /flushdns
call :LogResult "FlushDNS"
netsh winsock reset
call :LogResult "Winsock Reset"
netsh int ip reset
call :LogResult "IP Reset"

echo.
echo === MANTENIMIENTO COMPLETO FINALIZADO ===
echo.
echo === RESUMEN DEL LOG ===
type "%log%"
echo =========================
for /l %%i in (5,-1,1) do (
    echo Regresando al menu principal en %%i segundos...
    timeout /nobreak /t 1 >nul
)
goto MENU

:SYSINFO
echo ============================================
echo INFORMACION DEL SISTEMA
echo ============================================
echo [%date% %time%] Informacion del sistema >> "%log%"
echo.
echo Version de Windows:
ver
echo.
echo Nombre del equipo:
hostname
echo.
echo Espacio en disco:
wmic logicaldisk get caption,freespace,size
goto ReturnMenu