import boto3
import os
import time
import datetime
from datetime import datetime
from botocore.exceptions import ClientError

"""
Created by Kieran Coito
CSS 436
November 1st 2019

Assignment 3

This program will back up to AWS the active directory that the program is executed in. It will not upload itself or 
the metaData.txt it creates to facilitate the uploading.

"""
#name of file to save relevant info for uploading
metaDataText = "metaData.txt"

def backer():
    newSave = False
    print("starting program")
    #set up client to upload files, and create new bucket to upload files to
    client = boto3.client('s3')

    #check if there is already a meta data file or not, which dictates if this is the first run or not
    if not os.path.exists(metaDataText):
        metaData = open(metaDataText, "w")
        #get current time of application running and save it for future reference
        lastupdate = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
        metaData.write( str(lastupdate) + "\n")
        #create new bucket name and save it for future reference
        bucketName = "backupBucket_" + str(datetime.now().strftime('%Y%m%d%H%M%S'))
        metaData.write(bucketName)
        newSave = True
    else:
        metaData = open(metaDataText, "r+")
        #get time of last upload
        lastUpdatedTime = str(metaData.readline())
        if lastUpdatedTime != '':
            lastupdate = datetime.strptime(lastUpdatedTime[0:len(lastUpdatedTime)-1], '%Y-%m-%d %H:%M:%S')
        #get bucket name that is being backed up to
        bucketName = str(metaData.readline())
        metaData.seek(0)
        metaData.truncate()
        #update file to current time as new upload is happening
        metaData.write(datetime.now().strftime('%Y-%m-%d %H:%M:%S') + "\n")
        metaData.write(bucketName)

    s3 = boto3.resource('s3')
    if newSave:
        try:
            client.create_bucket(Bucket=bucketName, CreateBucketConfiguration={'LocationConstraint': 'us-west-2'}, )
        #handle specific exceptions that may happen
        except ClientError as e:
            if e.response['Error']['Code'] == 'BucketAlreadyExists':
                print("That bucket already exists and cannot be created and uploaded to")
                return 0
            if e.response['ResponseMetadata']['HTTPStatusCode'] == 500:
                print("Could not connect to service will retry")
                return -1
            else:
                print("An error occurred while trying to connect to the client, please check credentials and rerun")
                return 0

    print(" ")
    print("The bucket named " + bucketName + " will store your backup")
    print("upload starting")
    print(" ")
    #iterate through all files in the current directory and down
    for dirname, dirnames, filenames in os.walk('.'):
        for filename in filenames:

            fullpath = str(os.path.join(dirname, filename))
            length = len(fullpath)

            #the current file that will be uploaded, get last time it was updated
            currentFileUpdateTime = os.path.getmtime(fullpath)
            updateTime = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(currentFileUpdateTime))
            fileTime = datetime.strptime(updateTime, '%Y-%m-%d %H:%M:%S')

            #only upload of this is a new save or the file has been edited since the last upload
            if newSave or (fileTime > lastupdate):
                #attempt to upload file, catch any errors and handle appropriately
                try:
                    s3.Object(bucketName, 'Backups' + fullpath[1:length] )\
                        .put(Body=open(fullpath,"rb"))
                except ClientError as e:
                    if e.response['ResponseMetadata']['HTTPStatusCode'] == 500:
                        print("Could not connect to service will retry")
                        return -1
                    else:
                        print("An error occurred while trying to upload, "
                            "please check credentials and file permissions and rerun")

                print(filename + " uploaded to " 'Backups' + fullpath[1:length] )
    print("Uploading has finished")
    return 0

print("\n")
#run back up program
retry = backer()
#if backer returns a -1 than there was a connection issue and it should run backer one more time
if retry == -1:
    backer()