![deployment](https://github.com/weol/seatpicker/actions/workflows/deploy.yml/badge.svg)

# Seat reservation application for events

Seatpicker is a seat reservation application for events. It was created for use in the lan party [SaltenLAN](https://saltenlan.no). It displays a overview of all the available seats in a venue that users can reserve. Users can log in using their Discord account, once they are logged in they may reserve a seat, move their reservation, or delete their reservation; only one reservation can be made per user.

The application supports running multiple lans in parallel, each lan must be associated with a Discord server (referred to as a guild in code). A single Discord server can only have a single *active* lan at any given time and however many *inactive* lans. Once a user logs in to a lan they are automatically added to the Discord server of the lan.

There is also an admin functionality that allows admins to manage reservations such as adding, moving, and deletion of reservations. They can also create new lans and make changes to the seats and the venue.

## Project details

The application consists of a C# API and a React+Typescript frontend that is hosted in Azure. The API utilizes MartenDB for eventsourcing and document store. The frontend uses Material UI, and Recoil for state management.

## API structure
The API is designed with Domain-Driven Design, and is therefore split into 3 projects, the Domain project, the Application project, and the Infrastrucure project. The Domain project contains all the domain logic and models, whilst the Application layer manages data access & persistence using ports defined inte Application project. The Infrastructure project implements the ports in adapters, and contains the entrypoints to the application, as well as authentication, configuration, and all that startup jazz.

