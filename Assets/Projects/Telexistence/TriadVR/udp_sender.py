import triad_openvr
import time
import sys
import struct
import socket

sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
server_address = ('127.0.0.1', 8051)

defaultFreq=90

#Key (Tracker ID):[List of serial numbers]
IDAssignments={0:["LHR-1414C213"],1:["LHR-0EC2E61F"],2:["LHR-04DC8463"],3:["LHR-0EC2E12D"]}
DeviceAssignments={0:-1,1:-1,2:-1,3:-1}


v = triad_openvr.triad_openvr()
v.print_discovered_objects()

if len(sys.argv) == 1:
    interval = 1/float(defaultFreq)
elif len(sys.argv) == 2:
    interval = 1/float(sys.argv[1])
else:
    print("Invalid number of arguments")
    interval = False

def GetID(serial):
    for i in IDAssignments:
        for j in IDAssignments[i]:
            if j==serial:
                return i

    return -1


print "Detecting attached trackers.."
for name in v.object_names["Tracker"]:
    d=v.devices[name]
    ID=GetID(d.get_serial())
    DeviceAssignments[ID]=d
    print "Tracker detected with ID:"+ str(ID) +" - battery: "+ str(int(d.get_bettery()*100))+"%"


if interval:
    while(True):
        start = time.time()
        for ID in DeviceAssignments:
            d=DeviceAssignments[ID]
            if d==-1:
                continue
            data =  d.get_pose_quaternion()
            sent = sock.sendto(struct.pack('i'+'d'*(len(data)),ID, *data), server_address)
            #print(str(ID)+" : "+str(data))

        sleep_time = interval-(time.time()-start)
        if sleep_time>0:
            time.sleep(sleep_time)
