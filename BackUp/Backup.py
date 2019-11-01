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
bucketName = "bucket" + str(datetime.now().strftime('%Y%m%d%H%M%S'))

def backer():
    newSave = False
    print("starting program")
    #set up client to upload files, and create new bucket to upload files to
    client = boto3.client('s3')

    try:
        client.create_bucket(Bucket= bucketName, CreateBucketConfiguration={'LocationConstraint': 'us-west-2' },)
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
    print("The bucket named " + bucketName + " was created to store your backup")
    print(" ")

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
    print(" ")
    #iterate through all files in the current directory and down
    for dirname, dirnames, filenames in os.walk('.'):
        for filename in filenames:

            fullpath = str(os.path.join(dirname, filename))
            length = len(fullpath)

            #skip over metadata used for this file and this file itself
            if filename == "Backup.py" or filename == "metaData.txt":
                continue

            #the current file that will be uploaded, get last time it was updated
            currentFileUpdateTime = os.path.getmtime(fullpath)
            updateTime = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(currentFileUpdateTime))

            #only upload of this is a new save or the file has been edited since the last upload
            if newSave or updateTime > lastUpdatedTime:
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
    print(" ")
    print("Uploading has finished")
    return 0

print(" ")
print(" ")
print(" ")
#run back up program
retry = backer()
#if backer returns a -1 than there was a connection issue and it should run backer one more time
if retry == -1:
    backer()