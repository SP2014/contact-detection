"""
Junyang@CompEpi
Analyze hand position data.
Input data file and output data file are generated and stored in Data folder on Desktop.
"""
import matplotlib.pyplot as plt
import numpy as np
import os

from mpl_toolkits.mplot3d import Axes3D

#Globals
#Then calculate how many of them are false detection, that is, the time frames are too short to be an event
threshold = 0.1 # 0.1s, the threshold that decide if a frame is valid


user_name = os.environ['USERPROFILE']
data_path = user_name + "\\Desktop\\Data\\data_right.txt"
output_path = user_name + "\\Desktop\\Data\\output_right.txt"
output = open(output_path,'w')

file = open(data_path, "r")
lines = file.readlines()
length = (float)(lines[1].split()[2])
width = (float)(lines[1].split()[4])
height = (float)(lines[1].split()[6])

sorted_data = {}
skeleton_data = {}
event_data = {}
Total_event = 0
for line in lines:
    line = line.replace('|', ' ').split()
    if(line[0] == "IN" or line[0] == "ENTER"):
        if(sorted_data.has_key(line[1])):
            sorted_data[line[1]].append(line)
        else:
            sorted_data[line[1]] = [line] 
    elif(line[0] == "TOTAL"):
        Total_event = int(line[2])
    elif(line[0] == "SKT"):
        #store skeleton data in dic with ID as the key and line# of IN as value
        if(skeleton_data.has_key(line[7])):
            if(line[1] == "HAND_IN"):
                skeleton_data[line[7]] += 1
        else:
            skeleton_data[line[7]] = 0
    #For collecting data grouped by skeletons and sorted by entry
    if(line[0] == "ENTER"):
        if(event_data.has_key(int(line[8]))):
            event_data[int(line[8])][int(line[1])] = [line[2], int(line[7]) / 1000.0, 0]
        else:
            event_data[int(line[8])] = {}
            event_data[int(line[8])][int(line[1])] = [line[2], int(line[7]) / 1000.0, 0]
    elif(line[0] == "EXIT"):
        print(line)
        event_data[int(line[8])][int(line[1])][2] = int(line[7]) / 1000.0
        

previous = ""
#Define six dictionaries for six regions
LH = {}
RH = {}
LM = {}
RM = {}
LL = {}
RL = {}

#The postions for x,z,time are 3,5,7
for key in sorted_data.keys():
    for line in sorted_data[key]:
        # LL region
        if((float)(line[3]) < width/2 and (float)(line[5]) < length/3):
            if(previous != "LL"):
                if(LL.has_key(line[1])):
                    LL[line[1]].append([line[7]])
                else:
                    LL[line[1]] = [[line[7]]]
            elif(LL.has_key(line[1])):
                LL[line[1]][len(LL[line[1]]) - 1].append(line[7])
            else:
                LL[line[1]] = [[line[7]]]
            previous = "LL"
        # RL region
        if((float)(line[3]) > width/2 and (float)(line[5]) < length/3):
            if(previous != "RL"):
                if(RL.has_key(line[1])):
                    RL[line[1]].append([line[7]])
                else:
                    RL[line[1]] = [[line[7]]]
            elif(RL.has_key(line[1])):
                RL[line[1]][len(RL[line[1]]) - 1].append(line[7])
            else:
                RL[line[1]] = [[line[7]]]
            previous = "RL"
        # LM region
        if((float)(line[3]) < width/2 and (float)(line[5]) > length/3 and (float)(line[5]) < length*2/3):
            if(previous != "LM"):
                if(LM.has_key(line[1])):
                    LM[line[1]].append([line[7]])
                else:
                    LM[line[1]] = [[line[7]]]
            elif(LM.has_key(line[1])):
                LM[line[1]][len(LM[line[1]]) - 1].append(line[7])
            else:
                LM[line[1]] = [[line[7]]]
            previous = "LM"
        # RM region
        if((float)(line[3]) > width/2 and (float)(line[5]) > length/3 and (float)(line[5]) < length*2/3):
            if(previous != "RM"):
                if(RM.has_key(line[1])):
                    RM[line[1]].append([line[7]])
                else:
                    RM[line[1]] = [[line[7]]]
            elif(RM.has_key(line[1])):
                RM[line[1]][len(RM[line[1]]) - 1].append(line[7])
            else:
                RM[line[1]] = [[line[7]]]
            previous = "RM"
        # LH region
        if((float)(line[3]) < width/2 and (float)(line[5]) > length*2/3):
            if(previous != "LH"):
                if(LH.has_key(line[1])):
                    LH[line[1]].append([line[7]])
                else:
                    LH[line[1]] = [[line[7]]]
            elif(LH.has_key(line[1])):
                LH[line[1]][len(LH[line[1]]) - 1].append(line[7])
            else:
                LH[line[1]] = [[line[7]]]
            previous = "LH"
        # RH region
        if((float)(line[3]) > width/2 and (float)(line[5]) > length*2/3):
            if(previous != "RH"):
                if(RH.has_key(line[1])):
                    RH[line[1]].append([line[7]])
                else:
                    RH[line[1]] = [[line[7]]]
            elif(RH.has_key(line[1])):
                RH[line[1]][len(RH[line[1]]) - 1].append(line[7])
            else:
                RH[line[1]] = [[line[7]]]
            previous = "RH"
    previous = ""   #move to the next hand event, default the previous value

# Initialize the time for each region           
LL_time = 0    
RL_time = 0    
LM_time = 0    
RM_time = 0    
LH_time = 0    
RH_time = 0   
 
def div(n1,n2):
    if(n2 == 0):
        return "0"
    else:
        return str(n1/n2)
    
#Accumulate the time windwos for each region to get the total time
for key in LL.keys():
    for time in LL[key]:
        LL_time = LL_time + ((int)(time[-1]) - (int)(time[0]))
        
for key in RL.keys():
    for time in RL[key]:
        RL_time = RL_time + ((int)(time[-1]) - (int)(time[0]))

for key in LM.keys():
    for time in LM[key]:
        LM_time = LM_time + ((int)(time[-1]) - (int)(time[0]))              

for key in RM.keys():
    for time in RM[key]:
        RM_time = RM_time + ((int)(time[-1]) - (int)(time[0]))
        
for key in LH.keys():
    for time in LH[key]:
        LH_time = LH_time + ((int)(time[-1]) - (int)(time[0]))

for key in RH.keys():
    for time in RH[key]:
        RH_time = RH_time + ((int)(time[-1]) - (int)(time[0]))   
#Convert each time from milisecond to second
LL_time = LL_time / 1000.0
RL_time = RL_time / 1000.0
LM_time = LM_time / 1000.0
RM_time = RM_time / 1000.0
LH_time = LH_time / 1000.0
RH_time = RH_time / 1000.0
Total_time = LL_time + RL_time + LM_time + RM_time + LH_time + RH_time

#Analyze Regional Data
for key in LL:
    i = 0
    for line in LL[key]:
        LL[key][i] = [int(line[0]) / 1000.0, (int(line[len(line) - 1]) - int(line[0])) / 1000.0]
        i = i + 1

for key in RL:
    i = 0
    for line in RL[key]:
        RL[key][i] = [int(line[0]) / 1000.0, (int(line[len(line) - 1]) - int(line[0])) / 1000.0]
        i = i + 1
        
for key in LM:
    i = 0
    for line in LM[key]:
        LM[key][i] = [int(line[0]) / 1000.0, (int(line[len(line) - 1]) - int(line[0])) / 1000.0]
        i = i + 1
        
for key in RM:
    i = 0
    for line in RM[key]:
        RM[key][i] = [int(line[0]) / 1000.0, (int(line[len(line) - 1]) - int(line[0])) / 1000.0]
        i = i + 1
        
for key in LH:
    i = 0
    for line in LH[key]:
        LH[key][i] = [int(line[0]) / 1000.0, (int(line[len(line) - 1]) - int(line[0])) / 1000.0]
        i = i + 1
        
for key in RH:
    i = 0
    for line in RH[key]:
        RH[key][i] = [int(line[0]) / 1000.0, (int(line[len(line) - 1]) - int(line[0])) / 1000.0]
        i = i + 1

# get the number of events for each region
LL_count = 0 
for event in LL.keys():
    LL_count = LL_count + len(LL[event])
RL_count = 0 
for event in RL.keys():
    RL_count = RL_count + len(RL[event])
LM_count = 0 
for event in LM.keys():
    LM_count = LM_count + len(LM[event])
RM_count = 0 
for event in RM.keys():
    RM_count = RM_count + len(RM[event])
LH_count = 0 
for event in LH.keys():
    LH_count = LH_count + len(LH[event])
RH_count = 0 
for event in RH.keys():
    RH_count = RH_count + len(RH[event])
    

LL_false = 0
for event in LL.keys():
    for frame in LL[event]:
        if(frame[1] < threshold ):
            LL_false = LL_false + 1

RL_false = 0
for event in RL.keys():
    for frame in RL[event]:
        if(frame[1] < threshold ):
            RL_false = RL_false + 1  

LM_false = 0
for event in LM.keys():
    for frame in LM[event]:
        if(frame[1] < threshold ):
            LM_false = LM_false + 1 
            
RM_false = 0
for event in RM.keys():
    for frame in RM[event]:
        if(frame[1] < threshold ):
            RM_false = RM_false + 1 
                       
LH_false = 0
for event in LH.keys():
    for frame in LH[event]:
        if(frame[1] < threshold ):
            LH_false = LH_false + 1

RH_false = 0
for event in RH.keys():
    for frame in RH[event]:
        if(frame[1] < threshold ):
            RH_false = RH_false + 1
#Process Event Data, Form: {Ske:{event:[HT,ST,ET]}]
TWO_HANDS_EVENT = 0
for ske in event_data.keys():
    copy = event_data[ske]
    event_list = [[event, copy[event][1], copy[event][2]] for event in copy.keys()]
    i = 0 #interate through the event list  for each skeleton
    while i < (len(event_list) - 1):
        #if the exit itme of the former one is greater thant he enter time of the latter one
        if(event_list[i][2] > event_list[i+1][1]):
            TWO_HANDS_EVENT = TWO_HANDS_EVENT + 1
            i = i + 2
        else:
            i = i + 1
ONE_HAND_EVENT = Total_event - 2*TWO_HANDS_EVENT

#Visulization
l1 = [((float)(line.replace('|',' ').split()[3]),(float)(line.replace('|',' ').split()[4]),(float)(line.replace('|',' ').split()[5])) for line in lines if len(line.replace('|',' ').split()) == 9 and (line.replace('|',' ').split()[2] == "LH1")]
r1 = [((float)(line.replace('|',' ').split()[3]),(float)(line.replace('|',' ').split()[4]),(float)(line.replace('|',' ').split()[5])) for line in lines if len(line.replace('|',' ').split()) == 9 and (line.replace('|',' ').split()[2] == "RH1")]
l2 = [((float)(line.replace('|',' ').split()[3]),(float)(line.replace('|',' ').split()[4]),(float)(line.replace('|',' ').split()[5])) for line in lines if len(line.replace('|',' ').split()) == 9 and (line.replace('|',' ').split()[2] == "LH2")]
r2 = [((float)(line.replace('|',' ').split()[3]),(float)(line.replace('|',' ').split()[4]),(float)(line.replace('|',' ').split()[5])) for line in lines if len(line.replace('|',' ').split()) == 9 and (line.replace('|',' ').split()[2] == "RH2")]
#Left Hand 1
xs_l1 = [x for (x,_,_) in l1]
ys_l1 = [y for (_,y,_) in l1]
zs_l1 = [z for (_,_,z) in l1]
#Right Hand 1
xs_r1 = [x for (x,_,_) in r1]
ys_r1 = [y for (_,y,_) in r1]
zs_r1 = [z for (_,_,z) in r1]
#Left Hand 2
xs_l2 = [x for (x,_,_) in l2]
ys_l2 = [y for (_,y,_) in l2]
zs_l2 = [z for (_,_,z) in l2]
#Right Hand 2
xs_r2 = [x for (x,_,_) in r2]
ys_r2 = [y for (_,y,_) in r2]
zs_r2 = [z for (_,_,z) in r2]
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')
    
ax.scatter(xs_l1, zs_l1, ys_l1, c='g', marker='x')
ax.scatter(xs_r1, zs_r1, ys_r1, c='r', marker='x')
ax.scatter(xs_l2, zs_l2, ys_l2, c='y', marker='+')
ax.scatter(xs_r2, zs_r2, ys_r2, c='b', marker='+')
ax.set_xlim([0,2.5])
ax.set_ylim([0,2.5])
ax.set_zlim([0,2.5])# 1 m height of the box

#Draw the frame of the bed
plt.plot([0,width],[length,length])
plt.plot([width,width],[0,length])

VecStart_x = [0,     width,   0,        width,   width,  0,     0,     width, width,  0, 0,      0]
VecStart_y = [length,0,       length,   length,  0,      0,     0,     length,length, 0, 0,      0]
VecStart_z = [0,     0,       0,        0,       0,      height,     height,     height,     height,      0, 0,      0]
VecEnd_x = [width,   width,   0,        width,   width,  width, 0,     0,     width,  0, width,  0]
VecEnd_y = [length,  length,  length,   length,  0,      0,     length,length,0,      0, 0,      length]
VecEnd_z  =[0,       0,       height,        height,       height,      height,     height,     height,     height,      height, 0,      0]
for i in range(12):
    ax.plot([VecStart_x[i], VecEnd_x[i]], [VecStart_y[i],VecEnd_y[i]],zs=[VecStart_z[i],VecEnd_z[i]])
ax.set_xlabel('X Label')
ax.set_ylabel('z Label')
ax.set_zlabel('Y Label')
plt.show()

# Write Data to Output File

output.write("***************************HAND EVENT SUMMARY****************************\n")
output.write("TOTAL EVENT COUNT: " + str(Total_event) + "  TWO HANDS EVENT COUNT: " + str(TWO_HANDS_EVENT) + "; ONE HAND EVENT COUNT: " + str(ONE_HAND_EVENT) + "\n")
output.write("TOTAL TIME(s): " + str(Total_time) + "\n")
output.write("AVG TIME: " + div(Total_time, Total_event) + "\n")
output.write("============================REGION TABLE SUMMARY=========================\n")
output.write("REGION  |  TOTAL TIME(s)   |   EVENT COUNT  |  AVG TIME(s)\n")
output.write("LL:         " + str(LL_time) + "                     " + str(LL_count - LL_false) + "(" + str(LL_false) + ")" + "              " + div(LL_time,LL_count) + "\n")
output.write("RL:         " + str(RL_time) + "                     " + str(RL_count - RL_false) + "(" + str(RL_false) + ")" + "              " + div(RL_time,RL_count) + "\n")
output.write("LM:         " + str(LM_time) + "                     " + str(LM_count - LM_false) + "(" + str(LM_false) + ")" + "              " + div(LM_time,LM_count) + "\n")  
output.write("RM:         " + str(RM_time) + "                     " + str(RM_count - RM_false) + "(" + str(RM_false) + ")" + "              " + div(RM_time,RM_count) + "\n")
output.write("LH:         " + str(LH_time) + "                     " + str(LH_count - LH_false) + "(" + str(LH_false) + ")" + "              " + div(LH_time,LH_count) + "\n")
output.write("RH:         " + str(RH_time) + "                     " + str(RH_count - RH_false) + "(" + str(RH_false) + ")" + "              " + div(RH_time,RH_count) + "\n")
output.write("=========================================================================\n")
output.write("Notes: Green is LH1,Red is RH1, Yellow is LH2, Blue is RH2\n")
output.write("**************************Skeleton Data Summary**************************\n")
output.write("TOTAL SKELETONS: " + str(len(skeleton_data)) + "\n")
output.write("CONTACT SKELETONS: " + str(len([x for x in skeleton_data.values() if x > 0])) + "\n")
output.write("*************************************************************************\n")
output.write("************************SKELETON DATA DETAIL*****************************")
for ske in event_data.keys():
    output.write("\nSKELETON ID: " + str(ske) + "\n")
    output.write('\n'.join('{}:{}'.format(key, val) for key, val in event_data[ske].items()))
output.write("\n")
output.write("***********************REGION DATA DETAIL********************************\n")
output.write("LL:\n   ")
output.write('\n   '.join('{}:{}'.format(key, val) for key, val in LL.items()))
output.write("\nRL:\n   ")
output.write('\n   '.join('{}:{}'.format(key, val) for key, val in RL.items()))
output.write("\nLM:\n   ")
output.write('\n   '.join('{}:{}'.format(key, val) for key, val in LM.items()))
output.write("\nRM:\n   ")
output.write('\n   '.join('{}:{}'.format(key, val) for key, val in RM.items()))
output.write("\nLH:\n   ")
output.write('\n   '.join('{}:{}'.format(key, val) for key, val in LH.items()))
output.write("\nRH:\n   ")
output.write('\n   '.join('{}:{}'.format(key, val) for key, val in RH.items()))
output.write("\n*****************************END****************************************")
output.close()


        
 