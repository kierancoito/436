import boto3
import os
import time
import datetime
from datetime import datetime
from botocore.exceptions import ClientError

import http

"""
Initial File creation


"""

metaDataText = "metaData.txt"
bucketName = "bucket"

def backer():
    newSave = False
    print("starting program")
    #set up client to upload files, and create new bucket to upload files to
    client = boto3.client('s3')

    # todo check if bucket exists first
    # then if it doesn't exist create new bucket
    # os.environ['AWS_DEFAULT_REGION'] =
    try:
        client.create_bucket(Bucket= bucketName, CreateBucketConfiguration={'LocationConstraint': 'us-west-2' },)
    except ClientError as e:
        if e.response['Error']['Code'] == 'BucketAlreadyExists':
            print("That bucket already exists and cannot be created and uploaded to")
            return 0
        if e.response['ResponseMetadata']['HTTPStatusCode'] == 500:
            print("Could not connect to service will retry")
            return -1

    #check if there is already a meta data file or not, which dictates if this is the first run or not
    if not os.path.exists(metaDataText):
        metaData = open(metaDataText, "w+")
        metaData.write(datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
        newSave = True
    else:
        metaData = open(metaDataText, "w+")
        #get time of last upload
        lastUpdatedTime = metaData.readline()
        if lastUpdatedTime != '':
            datetime.strptime(lastUpdatedTime, '%Y-%m-%d %H:%M:%S')
        metaData.seek(0)
        metaData.truncate()
        #update file to current time as new upload is happening
        metaData.write(datetime.now().strftime('%Y-%m-%d %H:%M:%S'))

    s3 = boto3.resource('s3')

    print("uploading starting")
    #iterate through all files in the current directory and down
    for dirname, dirnames, filenames in os.walk('.'):
        for filename in filenames:
            #the current file that will be uploaded, get last time it was updated
            currentFileUpdateTime = os.path.getmtime(dirname + '/' + filename)
            updateTime = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(currentFileUpdateTime))
            #only upload of this is a new save or the file has been edited since the last upload
            if newSave or updateTime > lastUpdatedTime:
                length = len(dirname)
                try:
                    s3.Object(bucketName, 'Backup' + dirname[1:length] + '/' +filename )\
                        .put(Body=open(dirname + '/' + filename,"rb"))
                except ClientError as e:
                    if e.response['ResponseMetadata']['HTTPStatusCode'] == 500:
                        print("Could not connect to service will retry")
                        return -1

                print(dirname + '/' + filename + " uploaded to " 'Backup' + dirname[1:length] + "/")

    print("Uploading has finished")
    return 0


retry = backer()
if retry == -1:
    backer()