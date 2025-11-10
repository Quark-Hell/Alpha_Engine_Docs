#!/bin/bash
set -e

if [ ! -d "/src_project/project" ]; then
  git clone -b Architect2.0 --single-branch https://github.com/Quark-Hell/Alpha_Engine /src_project/project
fi

if [ ! -d "/doxygen-awesome-css" ]; then
  echo "Cloning doxygen-awesome-css..."
  git clone https://github.com/jothepro/doxygen-awesome-css.git /doxygen-awesome-css
fi

if [ ! -f "/src_project/Doxyfile" ]; then
  doxygen -g /src_project/Doxyfile

  # Graphviz settings
  sed -i "s|^#*DOT_TRANSPARENT.*=.*|DOT_TRANSPARENT        = YES|" /src_project/Doxyfile
  sed -i "s|^#*DOT_IMAGE_FORMAT.*=.*|DOT_IMAGE_FORMAT       = svg|" /src_project/Doxyfile

  # Doxygen settings
  sed -i "s|^OUTPUT_DIRECTORY.*=.*|OUTPUT_DIRECTORY     = /app/docs|" /src_project/Doxyfile
  sed -i "s|^INPUT.*=.*|INPUT							= /src_project/project/ALPHA_Engine/Engine|" /src_project/Doxyfile

  sed -i "s|^RECURSIVE.*=.*|RECURSIVE					= YES|" /src_project/Doxyfile
  sed -i "s|^GENERATE_HTML.*=.*|GENERATE_HTML			= YES|" /src_project/Doxyfile
  sed -i "s|^HTML_OUTPUT.*=.*|HTML_OUTPUT				= html|" /src_project/Doxyfile
  
  sed -i "s|^FILE_PATTERNS.*=.*|FILE_PATTERNS			= *.c *.cc *.cxx *.cpp *.c++ *.h *.hpp *.hxx|" /src_project/Doxyfile
  sed -i "s|^EXCLUDE_PATTERNS.*=.*|EXCLUDE_PATTERNS     = */[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/* */[Ee][Xx][Tt][Ee][Rr][Nn][Aa][Ll]/|" /src_project/Doxyfile

  sed -i "s|^PROJECT_NAME.*=.*|PROJECT_NAME				= \"Alpha Engine\"|" /src_project/Doxyfile
  sed -i "s|^PROJECT_LOGO.*=.*|PROJECT_LOGO				= /app/logos/Alpha_Engine_Logo_64.png|" /src_project/Doxyfile

  sed -i "s|^GENERATE_TREEVIEW.*=.*|GENERATE_TREEVIEW   = YES|" /src_project/Doxyfile
  sed -i "s|^DISABLE_INDEX.*=.*|DISABLE_INDEX			= NO|" /src_project/Doxyfile
  sed -i "s|^FULL_SIDEBAR.*=.*|FULL_SIDEBAR				= NO|" /src_project/Doxyfile
  sed -i "s|^HTML_COLORSTYLE.*=.*|HTML_COLORSTYLE       = LIGHT|" /src_project/Doxyfile

  # doxygen-awesome
  sed -i "s|^HTML_EXTRA_STYLESHEET.*=.*|HTML_EXTRA_STYLESHEET  = /doxygen-awesome-css/doxygen-awesome.css /doxygen-awesome-css/doxygen-awesome-sidebar-only.css|" /src_project/Doxyfile
  echo "HTML_EXTRA_STYLESHEET += /doxygen-awesome-css/doxygen-awesome-sidebar-only-darkmode-toggle.css" >> /src_project/Doxyfile
  sed -i "s|^HTML_EXTRA_FILES.*=.*|HTML_EXTRA_FILES       = /doxygen-awesome-css/doxygen-awesome-darkmode-toggle.js /doxygen-awesome-css/doxygen-awesome-fragment-copy-button.js /doxygen-awesome-css/doxygen-awesome-paragraph-link.js|" /src_project/Doxyfile

  # Graphviz
  sed -i "s|^#*DOT_TRANSPARENT.*=.*|DOT_TRANSPARENT				= YES|" /src_project/Doxyfile
  sed -i "s|^#*DOT_IMAGE_FORMAT.*=.*|DOT_IMAGE_FORMAT			= svg|" /src_project/Doxyfile
  sed -i "s|^#*DOT_GRAPH_MAX_NODES.*=.*|DOT_GRAPH_MAX_NODES     = 100|" /src_project/Doxyfile
  sed -i "s|^#*DOT_FONTPATH.*=.*|DOT_FONTPATH					= /usr/share/fonts/truetype/dejavu|" /src_project/Doxyfile
  sed -i "s|^#*DOT_FONTNAME.*=.*|DOT_FONTNAME					= DejaVu Sans|" /src_project/Doxyfile
  sed -i "s|^#*DOT_FONTSIZE.*=.*|DOT_FONTSIZE					= 10|" /src_project/Doxyfile
  sed -i "s|^#*DOT_BG_COLOR.*=.*|DOT_BG_COLOR					= transparent|" /src_project/Doxyfile
  sed -i "s|^#*DOT_EDGE_COLOR.*=.*|DOT_EDGE_COLOR				= \"#cccccc\"|" /src_project/Doxyfile
  sed -i "s|^#*DOT_NODE_COLOR.*=.*|DOT_NODE_COLOR				= \"#eeeeee\"|" /src_project/Doxyfile
  sed -i "s|^#*DOT_TEXT_COLOR.*=.*|DOT_TEXT_COLOR				= \"#dddddd\"|" /src_project/Doxyfile
  sed -i "s|^#*DOT_FONTCOLOR.*=.*|DOT_FONTCOLOR					= \"#dddddd\"|" /src_project/Doxyfile
fi

echo "Generating documentation..."
doxygen /src_project/Doxyfile

echo "Starting application..."
dotnet DocsGenerator.dll