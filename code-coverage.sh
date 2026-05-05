#!/bin/bash

set -e

echo "===================================="
echo "Running unit tests with coverage..."
echo "===================================="

dotnet test src/CleanArchitecture.UnitTests/CleanArchitecture.UnitTests.csproj \
  --collect:"XPlat Code Coverage" \
  --settings coverlet.runsettings

echo "===================================="
echo "Installing / updating ReportGenerator..."
echo "===================================="

dotnet tool update -g dotnet-reportgenerator-globaltool || \
dotnet tool install -g dotnet-reportgenerator-globaltool

echo "===================================="
echo "Generating HTML coverage report..."
echo "===================================="

reportgenerator \
  -reports:"src/CleanArchitecture.UnitTests/TestResults/*/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html

echo "===================================="
echo "Opening HTML coverage report..."
echo "===================================="

open coverage-report/index.html
