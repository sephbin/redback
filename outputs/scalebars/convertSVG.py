from svglib.svglib import svg2rlg
from reportlab.graphics import renderPDF, renderPM
drawing = svg2rlg("scale-bar-200-small.svg")

print(dir(drawing))
print(drawing.width)

renderPDF.drawToFile(drawing, "file.pdf")

