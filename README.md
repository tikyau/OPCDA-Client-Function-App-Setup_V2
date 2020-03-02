# OPCDA-Client-Function-App-Setup

![Device Twin](https://user-images.githubusercontent.com/17831550/75311009-61f1e880-5890-11ea-9198-b62fe4bc738d.png)

Device Twin & Azure Function are used for configuration managements through the cloud.

A replica of the configuration file will be stored in Blob.

An Az function will be listening to file changes in Blob and generate a blob SAS token which will be passed to the Device Twin attribute. Client app will receive the device twin notification, download and consume the latest config file without disconnection. 
