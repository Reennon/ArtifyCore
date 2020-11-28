# 1)Your program will only accept one argument - path to the image.
# Use sys.argv[1] to get it.

# 2)Then you can do any manipulations on the image

# 3)Finally you need to save the changes in the same file


# For example:

import sys
import os
import cv2
import json

# распарсенная строка
# parsed_string = sys.argv[1]
parsed_string = """{
  "numberOfImages": 2,
  "numberOfImage": [
    {

      "Name": "D:/ProjArtify/ArtifyCore/Users/Yura/image1.png"

    },
    {
      "Name": "D:/ProjArtify/ArtifyCore/Users/Yura/image2.png"

    }
  ],
  "orderCompleted": true
}"""
parsed_string = json.loads(parsed_string)
numberOfImage = int(parsed_string['numberOfImages'])
arr = []
for i in range(int(numberOfImage)):
    arr.append(parsed_string["numberOfImage"][1]["Name"])
# Your image
img = cv2.imread(arr[0])

# algorithms
img = cv2.rotate(img, cv2.cv2.ROTATE_90_CLOCKWISE)

# Store the resultant image
status = cv2.imwrite(arr[0], img)