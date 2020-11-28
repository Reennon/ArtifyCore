import numpy as np
import os


#from random import randint
#l = [randint(10,80) for x in range(10)]


def inverse(C):
    E = 0.01
    length = len(C)
    det = np.linalg.det(C)
    print(det)
    matrixx = [[0 for x in range(length)] for y in range(length)]
    inv_mas = [[0 for x in range(length)] for y in range(length)]
    mas1 = [[0 for x in range(length)] for y in range(length)]
    for i in range(length):  # список C копіюємо в другий список
        for j in range(length):
                matrixx[i][j] = C[i][j]
    for u in range(length):
      m = [[0 for x in range(length)] for y in range(length)]#створюємо додаткові списки
      b = [[0 for x in range(length)] for y in range(length)]
      x = [0 for x in range(length)]

      
    f = open('tex.txt', 'a')

    f.write("Похибка: " + str(Z**0.5))
    f.write('\n')
    f.write('\n')
    f.close()
    return inv_mas

#handle = open("matrix.txt", "r")#відкриваємо файл
#data = handle.readlines()#зчитуємо
#print(data)
#word_list = []
#for i in range(len(data)):
 #      word_list.append(data[i].split())
#запис матриць в змінні
#A = [[int(j) for j in word_list[i]] for i in range(4)]
##B = [[int(j) for j in word_list[i+5]] for i in range(3)]
#C = [[int(j) for j in word_list[i+9]] for i in range(4)]
#handle.close()
#k = np.random.random ([100,100]) * 10

#inverse(inverse(A))
###inverse(B)
#inverse(C)
#inverse(k)
print(11111111111111111)
f = open('D:\\tex.txt', 'a')

f.write("Похибка: " + str(0.7))
f.write('\n')
f.write('\n')
f.close()
os.system('cmd /k "date"')

