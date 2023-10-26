import * as React from 'react';
import {useEffect, useState} from 'react';
import {
  Box,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Stack
} from '@mui/material';
import background from "../Media/background.svg"
import {useUserContext} from "../UserContext";
import {useAlertContext} from "../AlertContext";
import Button from "@mui/material/Button";
import Seat from '../Models/Seat';
import {CookiesAdapter} from "../Adapters/CookiesAdapter";
import SeatAdapter from "../Adapters/SeatAdapter";
import StaticSeats from '../StaticSeats';
import Typography from "@mui/material/Typography";

export default function LanManagement() {
  return (
      <Stack sx={{my: 4, alignItems: 'center'}}>
        <Typography variant="h1" component="h1" gutterBottom>
          HELLO
        </Typography>
      </Stack>
  )
}
