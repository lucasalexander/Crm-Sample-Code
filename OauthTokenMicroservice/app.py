import requests
import json
import datetime
import time
from flask import Flask, request

#set some global variables

#resource name from AAD
resourcename = 'https://XXXXXXXXXXX.crm.dynamics.com'

#client name from AAD
clientid = 'XXXXXXXXXXX'

#token endpoint from AAD
tokenendpoint = 'https://login.microsoftonline.com/XXXXXXXXXXX/oauth2/token'

#length of time in seconds before token expires to request a refresh
timebeforerefresh = 1800 #1800 is 30 minutes

#port number on which to listen
portnumber = 5000

###DO NOT EDIT BELOW THIS LINE###

class Expando(object):
    pass

class Token(object):
    def __init__(self, accesstoken=None, refreshtoken=None, expireson=None, username=None, password=None):
        self.accesstoken = accesstoken
        self.refreshtoken = refreshtoken
        self.expireson = expireson
        self.username = username
        self.password = password

#create an empty list to hold retrieved tokens
tokens = []

#set the datetime display format
timeformat = "%Y-%m-%d %H:%M:%S"

app = Flask(__name__)

@app.route('/requesttoken',methods=['GET', 'POST'])
def requesttoken():
    #get the user request
    userreq = request.get_json(silent=True)
    
    #print(len(tokens))

    #check if there's an existing token 
    existingtokens = list(filter(lambda x:x.username == userreq['username'] and x.password == userreq['password'], tokens))
    
    #if there is an existing token
    if len(existingtokens)>0:
        existingtoken = existingtokens[0]

        #check difference between expiration time and current time
        #if it's less than the timebeforerefresh value, then refresh it
        if (float(existingtoken.expireson)-time.time()) < timebeforerefresh:
            #create a refresh request
            tokenpost = {
                'client_id':clientid,
                'resource':resourcename,
                'refresh_token':existingtoken.refreshtoken,
                'grant_type':'refresh_token'
            }

            #make the token refresh request
            tokenres = requests.post(tokenendpoint, data=tokenpost)
            accesstoken = ''

            #extract the access token
            try:
                accesstoken = tokenres.json()['access_token']
                t = datetime.datetime.utcfromtimestamp(float(tokenres.json()['expires_on']))

                newtoken = Token()
                newtoken.accesstoken = tokenres.json()['access_token']
                newtoken.refreshtoken = tokenres.json()['refresh_token']
                newtoken.username = userreq['username']
                newtoken.password = userreq['password']
                newtoken.expireson = tokenres.json()['expires_on']

                #remove the old token
                tokens.remove(existingtoken)

                #cache the new token
                tokens.append(newtoken)
                
                #return the token details to the requestor
                tokenobj = Expando()
                tokenobj.token = tokenres.json()['access_token']
                tokenobj.expires_on = t.strftime(timeformat)
                tokenobj.action = "refreshed existing token"
                response = json.dumps(tokenobj.__dict__)
                return response

            except:
                try:
                    #if we received an error message from the endpoint, handle it
                    errorobj = Expando()
                    errorobj.error = tokenres.json()['error']
                    errorobj.description = tokenres.json()['error_description']
                    response = json.dumps(errorobj.__dict__)
                    return response
                except:
                    #for all other errors, return "unknon error"
                    return '{"error":"unknown error"}'

        else:
            #the cached token is still ok, so return it to the user
            t = datetime.datetime.utcfromtimestamp(float(existingtoken.expireson))
            tokenobj = Expando()
            tokenobj.token = existingtoken.accesstoken
            tokenobj.expires_on = t.strftime(timeformat)
            tokenobj.action = "returned existing token"
            response = json.dumps(tokenobj.__dict__)
            return response

    else:
        #no cached token, so need to request a new one
        #build the authorization request
        tokenpost = {
            'client_id':clientid,
            'resource':resourcename,
            'username':userreq['username'],
            'password':userreq['password'],
            'grant_type':'password'
        }

        #make the token request
        tokenres = requests.post(tokenendpoint, data=tokenpost)
        accesstoken = ''

        #extract the access token
        try:
            accesstoken = tokenres.json()['access_token']
            t = datetime.datetime.utcfromtimestamp(float(tokenres.json()['expires_on']))
            newtoken = Token()
            newtoken.accesstoken = tokenres.json()['access_token']
            newtoken.refreshtoken = tokenres.json()['refresh_token']
            newtoken.username = userreq['username']
            newtoken.password = userreq['password']
            newtoken.expireson = tokenres.json()['expires_on']

            #cache the token
            tokens.append(newtoken)
            
            #return the token details to the requester
            tokenobj = Expando()
            tokenobj.token = tokenres.json()['access_token']
            tokenobj.expires_on = t.strftime(timeformat)
            tokenobj.action = "retrieved new token"
            response = json.dumps(tokenobj.__dict__)
            return response

        except:
            try:
            #if we received an error message from the endpoint, handle it
                errorobj = Expando()
                errorobj.error = tokenres.json()['error']
                errorobj.description = tokenres.json()['error_description']
                response = json.dumps(errorobj.__dict__)
                return response
            except:
                #for all other errors, return "unknown error"
                return '{"error":"unknown error"}'

if __name__ == '__main__':
    app.run(debug=False,host='0.0.0.0',port=portnumber)