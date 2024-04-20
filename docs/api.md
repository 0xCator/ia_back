## User

### ``api/User/Login`` - POST
Used to log the user in
#### Request
```json
{
	"username": "
	"password": ""
}
```
#### Response
The response contains the user's information (and an auth token once implemented).

### ``api/User/Register`` - POST
Used to register a new user
#### Request
```json
{
	"username": "",
	"password": "",
	"email": "",
	"name": "",
}
```
#### Response
The response contains the user's information (and an auth token once implemented).

## Project

### ``api/Project/`` - PUT
Creates a new project
#### Request
```json
{
	"name": "",
	"TeamLeaderID": "",
	"RequestedDevelopers": []
}
```
#### Response
The response contains the project's information.

### ``api/Project/id`` - DELETE
Deletes a project
#### Request
```empty```
#### Response
Empty response