#!/usr/bin/python3

import os
import subprocess 

path =  os.path.dirname(os.path.realpath(__file__)) + "/../bin/Debug/net5.0/acr"
acr = os.path.realpath(path)

commands = [""];
while(len(commands) > 0):
	#print(commands)
	item  = commands.pop(0);
	found_commands = False;
	args = [acr];
	if(item != ""):
		for c in item.split(" "):
			args.append(c)
	args.append('--help')
	children = []
	print("\n## {}\n".format(item.capitalize() or "Usage"))
	print("```bash")
	for line in subprocess.check_output(args).decode().split('\n'):
		print(line)
		if (line.lower() == "commands:"):
			found_commands = True;
			continue
		if(found_commands == True):
			sub_command = line.strip(' ').split(' ')[0]
			if(sub_command != ""):
				if(item != ""):
					sub_command = item + " " + sub_command
				print(sub_command)
				children.append(sub_command)
	commands = children + commands
	print("```")
