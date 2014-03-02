Patient Contact Detection System
=================
An Kinect application that can detect and record the activities of people in the detection zone by tracking the positions of their hands as well as 
the general positions of their bodies. Specifically, the activities of the hands in the region 
of bed are classified as patient contacts, which is what the system focuses on analyzing.

Hardware Requirement
=================
• Two Kinects (first generation)
• Two considerably large fiducial markers
• Two computers with USD 3.0 host controllers installed.

Software Requirement
=================
• Canopy
• Microsoft VIsual Studio 2012
• Microsoft Visual C++ 2010 Redistributable Package (x86)
• Microsoft Kinect SDK 1.8

Setting
=================
Assume a table is the hand tacking zone with one side treated as the head and the opposite side treated as the end.
1. Attach two fiducial markers to a board and place the board at the end of the table.
2. Place two Kinects at the opposite sides of head of the table.
3. Place the two Kinects to point to the fiducial markers.
4. Connect the Kinects to the two computers. Each computer controls a Kinect.

Instructions
=================
0. Create a folder called "Data" on Desktop for each computer.
1. Open the project in two computers, select the placement for the Kinect the computer is connecting to.
2. After a while, the hand tacking zone will be created and visualized on UI. The system is start collecting the data.
3. After closing the system, the data files will be generated and stored in the "Data" folder on Desktop.
4. Use corresponding python program to analyze the the data and generate a "output.txt" file under the same folder for each computer.
* You can just use one computer with 2 USB 3.0 host controllers instead of using two computer, the only difference is that you open the project twice to achieve exactly the same result.