import sys
sys.path.append("..") # Adds higher directory to python modules path.
import pyTableMaker as tm

data = [
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/angel.png", "imageHeight":5},{"value":"Header"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png", "imageHeight":25},{"value":"A"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/shugenja.png", "imageHeight":25},{"value":"B"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/heretic.png", "imageHeight":25},{"value":"C"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png", "imageHeight":25},{"value":"A"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/shugenja.png", "imageHeight":25},{"value":"B"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/heretic.png", "imageHeight":25},{"value":"C"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png", "imageHeight":25},{"value":"A"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/shugenja.png", "imageHeight":25},{"value":"B"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/heretic.png", "imageHeight":25},{"value":"C"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png", "imageHeight":25},{"value":"A"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/shugenja.png", "imageHeight":25},{"value":"B"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/heretic.png", "imageHeight":25},{"value":"C"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png", "imageHeight":25},{"value":"A"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/shugenja.png", "imageHeight":25},{"value":"B"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/heretic.png", "imageHeight":25},{"value":"C"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/ojo.png", "imageHeight":25},{"value":"A"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/shugenja.png", "imageHeight":25},{"value":"B"},],
[{"image":r"C:/Users/Andrew.Butler/OneDrive%20-%20Cox%20Architecture%20Pty%20Ltd/RPG/BotCT/Icons/mod/heretic.png", "imageHeight":25},{"value":"C"},],
]

table = tm.table(data, {"headerCount":1})

# print(table)


with open("testing.icml", "w") as file:
	file.write(str(table))