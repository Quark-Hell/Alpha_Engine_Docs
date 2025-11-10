#!/bin/bash
set -e

if [ ! -d "/src_project/project" ]; then
  git clone -b Architect2.0 --single-branch https://github.com/Quark-Hell/Alpha_Engine /src_project/project
fi

if [ ! -f "/src_project/Doxyfile" ]; then
  doxygen -g /src_project/Doxyfile
  
  # Doxygen settings
  sed -i "s|^OUTPUT_DIRECTORY.*=.*|OUTPUT_DIRECTORY     = /app/docs|" /src_project/Doxyfile
  sed -i "s|^RECURSIVE.*=.*|RECURSIVE					= YES|" /src_project/Doxyfile
  sed -i "s|^GENERATE_HTML.*=.*|GENERATE_HTML			= YES|" /src_project/Doxyfile
  sed -i "s|^HTML_OUTPUT.*=.*|HTML_OUTPUT				= html|" /src_project/Doxyfile
  sed -i "s|^PROJECT_NAME.*=.*|PROJECT_NAME				= \"Alpha Engine\"|" /src_project/Doxyfile
  sed -i "s|^INPUT.*=.*|INPUT							= /src_project/project/ALPHA_Engine/Engine|" /src_project/Doxyfile
  sed -i "s|^FILE_PATTERNS.*=.*|FILE_PATTERNS			= *.c *.cc *.cxx *.cpp *.c++ *.h *.hpp *.hxx|" /src_project/Doxyfile
  sed -i "s|^EXCLUDE_PATTERNS.*=.*|EXCLUDE_PATTERNS     = */[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/* */[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/|" /src_project/Doxyfile
fi

echo "Generating documentation..."
doxygen /src_project/Doxyfile

echo "Starting application..."
dotnet DocsGenerator.dll