# GlueHome - Platform Technical Task

---

### Do not fork this repo! By doing so other candidates will be able to see your solution

---

## Background

Your company has decided to create a delivery system to connect users from the consumer market to partners from the logistics business sector.

You are responsible for building a Web API that will be used by partners and users to create, manage and execute deliveries.

### Business Requirements

* The API should support all CRUD operations (create, read, update, delete).
* A delivery must handle 5 different states: `created`, `approved`, `completed`, `cancelled`, `expired`
* Users may `approve` a delivery before it starts.
* Partner may `complete` a delivery, that is already in `approved` state.
* If a delivery is not `completed` during the access window period, then it should expire. 
* Either the partner or the user should be able to cancel a pending delivery (in state `created` or `approved`).

A delivery should respect the following structrure:

```json
{
    "state": "created",
    "accessWindow": {
        "startTime": "2019-12-13T09:00:00Z",
        "endTime": "2019-12-13T11:00:00Z"
    },
    "recipient": {
        "name": "John Doe",
        "address": "Merchant Road, London",
        "email": "john.doe@mail.com",
        "phoneNumber": "+44123123123"
    },
    "order": {
        "orderNumber": "12209667",
        "sender": "Ikea"
    }
}
```

### Technical Requirements

* All code should be written in C# and target the .NET core, or .NET framework library version 4.5+.
* Please check all code into a public accessible repository on GitHub and send us a link to your repository.

Feel free to make any assumptions whenever you are not certain about the requirements, but make sure your assumptions are made clear either through the design or additional documentation.

### Bonus
* Application logging
* Documentation
* Containerization
* Authentication
* Testing
* Data storage
* Partner facing Pub/Sub API for receiving state updates.
* Anything else you feel may benefit your solution from a technical perspective.
