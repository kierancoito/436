import boto3
import os
import time
import datetime

"""
Initial File creation


"""

metaDataText = "metaData.txt"
bucketName = "backUpForClass"

def main(SECRET_KEY, ACCESS_KEY):
    newSave = False
    #set up client to upload files, and create new bucket to upload files to
    client = boto3.client('s3', aws_access_key_id=ACCESS_KEY, aws_secret_access_key=SECRET_KEY)

    # todo check if bucket exists first
    # then if it doesn't exist create new bucket
    client.create_bucket(Bucket='currentBackup')

    #check if there is already a meta data file or not, which dictates if this is the first run or not
    if not os.path.exists(metaDataText):
        metaData = open(metaDataText, "w+")
        metaData.writable(datetime.datetime.now())
        newSave = True
    else:
        metaData = open(metaDataText, "w+")
        #get time of last upload
        lastUpdatedTime = metaData.readline()
        metaData.seek(0)
        metaData.truncate()
        #update file to current time as new upload is happening
        metaData.writable(datetime.datetime.now())

    s3 = boto3.resource('s3')

    #iterate through all files in the current directory and down
    for dirname, dirnames, filenames in os.walk('.'):

        #set the key for each file as the directory path
        for filename in filenames:

            #the current file that will be uploaded, get last time it was updated
            currentFileUpdateTime = os.path.getmtime(os.path.join(dir, filename))
            updateTime = time.strftime('%Y-%m-%d %H:%M:%S', time.localtime(currentFileUpdateTime))

            #only upload of this is a new save or the file has been edited since the last upload
            if newSave or updateTime > lastUpdatedTime:
                s3.meta.client.upload_file(os.path.join(dir, filename), 'currentBackup', dirname + '/')