"""
Junyang@CompEpi
Plotting the coordinates data set of the hands. 
"""
import matplotlib.pyplot as plt

file = open("C:\\Users\\uiowa-J\\Desktop\\CompEpi\\knect-new\\windows_c#\\Two_Kinects(Marker&Single) - Data\\DistanceDetection\\bin\\Debug\\data.txt", "r")
lines = file.readlines()
lot = []
for line in lines:
    if(len(line.split()) == 4):
        lot.append(tuple(float(s) for s in line.split()[3][1:-1].split(',')))

x = [x for (x,_) in lot]
z = [z for (_,z) in lot]
plt.plot([0,1],[2.3,2.3])
plt.plot([1,1],[0,2.3])
plt.plot(x, z, 'g--')
plt.axis([0, 3, 0, 3])
plt.show()
