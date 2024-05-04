# Freely accessed endpoints

#### GET ``/api/User/login``
Used to log in a user.
Expected body:
```json
{
	"username": "username",
	"password": "password"
}
```

#### GET ``/api/User/Register``
Used to register a user. The username and email must be unique.
The user is not logged in after registration.
Expected body:
```json
{
	"username": "username",
	"password": "password",
	"email": "email",
	"name": "name"
}
```


# Authenticated endpoints

## Projects

### Creating a project
#### POST ``/api/Project``
Used to create a project.
The user is assigned as a team leader and cannot be a developer.
Requested developers could be an empty array.
Expected body:
```json
{
	"name": "name",
	"teamleaderid": "teamleaderid"
	"requesteddevelopers": ["developerid1", "developerid2"]"
}
```

### Getting all projects for a user
#### GET ``/api/Project/user/[id]``
Used to get all projects for a single user. In card format (id and name).
Could be used with the websocket to update the user on new projects.
Expected response:
```json
{
	"createdprojects": [
		{
			"id": "id",
			"name": "name"
		},
		{
			"id": "id",
			"name": "name"
		}
	],
	"assignedprojects": [
		{
			"id": "id",
			"name": "name"
		}
	],
	"projectrequests": []
}
```

### Getting a project
#### GET ``/api/Project/[id]``
Used to get a single project. With all the details (teamleader id, developers' info).
Expected response:
```json
{
	"id": "id",
	"name": "name",
	"teamleaderid": "teamleaderid",
	"assigneddevelopers": [
		{
			"id": "id",
			"name": "name",
			"username": "username"
		}
	]
}
```

### Updating a project's name
#### PATCH ``/api/Project/[id]/[newname]``
Used to update a project's name.

### Deleting a project
#### DELETE ``/api/Project/[id]``
Used to delete a project.

### Adding a developer to a project
#### POST ``/api/Project/[id]/developer/[developerUsername]``
Used to add a developer to a project. The developer must accept the request.
The developer must not be a team leader and the developer must not be already assigned to the project.

### Accepting a project request
#### POST ``/api/User/acceptRequest``
Used to accept a project request. The project must be in the user's project requests.
Expected body:
```json
{
	"projectid": "projectid",
	"userid": "userid"
}
```

### Declining a project request
#### POST ``/api/User/rejectRequest``
Used to decline a project request. The project must be in the user's project requests.
Expected body:
```json
{
	"projectid": "projectid",
	"userid": "userid"
}
```

### Removing a developer from a project
#### DELETE ``/api/Project/[id]/developer/[developerUsername]``
Used to remove a developer from a project. The developer must be assigned to the project.

## Tasks

### Creating a task
#### POST ``/api/ProjectTask``
Used to create a task with a status of "To Do".
The developer must be assigned to the project.
Expected body:
```json
{
	"name": "name",
	"description": "description",
	"projectid": "projectid",
	"assigneddevid": "developerid"
}
```

### Getting all tasks for a project
#### GET ``/api/ProjectTask/project/[id]``
Used to get all tasks for a project. In card format (id, name, description, status, assigned developer info).
Expected response:
```json
[
	{
		"id": "id",
		"name": "name",
		"description": "description",
		"status": "status",
		"assigneddev": {
			"id": "id",
			name: "name",
			username: "username"
		}
	}
]
```

### Getting a task
#### GET ``/api/ProjectTask/[id]``
Used to get a single task. With all the details (name, description, status, attachment, assigned developer info).
Expected response:
```json
{
	"id": "id",
	"name": "name",
	"description": "description",
	"status": "status",
	"attachment": "attachment",
	"assigneddev": {
		"id": "id",
		name: "name",
		username: "username"
	}
}
```

### Updating a task's status
#### PATCH ``/api/ProjectTask/[id]``
Used to update a task's status. The status must be one of the following: 0 = To Do, 1 = In Progress, 2 = Done.
Expected body:
```json
{
	"status": "status"
}
```

### Uploading an attachment to a task
#### POST ``/api/ProjectTask/[id]/uploadattachment``
Used to upload an attachment to a task. Only on tasks with status "Done".
*Use FormData when utilizing React.*

### Getting an attachment from a task
#### GET ``/api/ProjectTask/[id]/attachmentfile``
Used to get an attachment from a task. Only on tasks with status "Done".

### Getting an attachment file's name from a task
#### GET ``/api/ProjectTask/[id]/attachmentname``
Used to get an attachment file's name from a task. Only on tasks with status "Done".


## Comments

### Creating a comment
#### POST ``/api/Comment``
Used to create a comment. The user must be assigned to the project.
Parent comment id is optional.
Expected body:
```json
{
	"content": "content",
	"taskid": "taskid",
	"userid": "userid"
	"parentcommentid": "parentcommentid"
}
```

### Getting all comments for a task
#### GET ``/api/Comment/task/[id]``
Used to get all comments for a task. In card format (id, comment, user info).
Expected response:
```json
[
	{
		"id": "id",
		"content": "content",
		"parentcommentid": "parentcommentid",
		"commenterInfo": {
			"id": "id",
			"name": "name",
			"username": "username"
		}
	}
]
```

### Getting a comment
#### GET ``/api/Comment/[id]``
Used to get a single comment. With all the details (content, user info).
Expected response:
```json
{
	"id": "id",
	"content": "content",
	"commenterInfo": {
		"id": "id",
		"name": "name",
		"username": "username"
	}
}
```