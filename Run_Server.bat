@echo off
title Tay Du Ky Mobile - Mock Server
color 0A
echo ==========================================================
echo       MAY CHU GIA LAP - TAY DU KY MOBILE (J2ME)
echo ==========================================================
echo.
echo IP Lang nghe: 0.0.0.0
echo Port TCP: 8080
echo.
echo Dang kiem tra Python...
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [LOI] Khong tim thay Python tren he thong!
    echo Vui long tai va cai dat Python tai: https://www.python.org/downloads/
    echo Va tick vao o "Add Python to PATH" khi cai dat.
    echo.
    pause
    exit /b
)

echo Dang khoi chay server...
echo ----------------------------------------------------------
python D:\AgentAI\TayDuKy2D\server.py
if %errorlevel% neq 0 (
    echo [LOI] May chu dung dot ngot voi loi!
    pause
)
