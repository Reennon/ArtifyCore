# 1)Initialize: accept one argument - path to the images.

# 2)Return_result: return json via pipeline


# For example:
# Your json:
#parsed_string = """{
#   "NumberOfImages": 2,
#   "numberOfImage": [
#     "D:/ProjArtify/ArtifyCore/Users/Yura/image1.png",
#     "D:/ProjArtify/ArtifyCore/Users/Yura/image2.png"
#   ],
#   "PathToModule": "enemy"
# }"""


import json
import sys


def get():

    #Input data(we parse it)
    app_json = json.loads(sys.argv[1])
 
    #Number of image
    numberOfImage = app_json['NumberOfImages']
    arr = []

    #Add your filePATH to array
    for i in range(numberOfImage):
        arr.append(app_json['Image'][i])
    return arr

def return_result(status = (0), error = 0):
        dict={}
        dict['EEError'] = error
        dict['Result'] = status
        
        #Return Json-Results
        app_json = json.dumps(dict)
        f = open(r'\\.\pipe\NPtes', 'r+b', 0)
        f.write(app_json.encode())
        f.seek(0)
