#!/usr/bin/python3

from random import random

def remove_rows(filename):
    with open(filename, 'r') as i:
        indata = i.readlines()

    outdata = []
    outdata.append(indata[0])
    [outdata.append(u) for u in indata[1:] if random() > 0.1]

    with open(filename.split('.')[0] + '_missingrows.csv', 'w') as o:
        o.writelines(outdata)


if __name__ == '__main__':
    remove_rows('SPY.csv')
    remove_rows('QQQ.csv')
