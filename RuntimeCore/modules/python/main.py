


#1)Your program will only accept one argument - path to the image.
#Use sys.argv[1] to get it.

#2)Then you can do any manipulations on the image

#3)Finally you need to save the changes in the same file


#For example:

import sys
import os
import cv2


#Your image
img = cv2.imread(sys.argv[1])

#algorithms
img = cv2.rotate(img, cv2.cv2.ROTATE_90_CLOCKWISE)

# Store the resultant image
status = cv2.imwrite(sys.argv[1], img)




