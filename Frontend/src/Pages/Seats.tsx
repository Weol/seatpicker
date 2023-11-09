import * as React from 'react';
import {useEffect, useState} from 'react';
import {
  Box,
  Stack
} from '@mui/material';
import background from "../Media/background.svg"
import {useAlerts} from "../AlertContext";
import SeatComponent from '../Components/SeatComponent';
import Seat from '../Models/Seat';
import createSeats from "../StaticSeats";
import useSeats from "../SeatsHook"
import useReservation from "../ReservationHook"
import useLoggedInUser from "../LoggedInUserHook";
import { useDialogs } from '../DialogContext';

export default function Seats() {
  const { alertWarning, alertInfo, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const loggedInUser = useLoggedInUser()
  const { seats, reloadSeats, reservedSeat, createNewSeat } = useSeats()
  const { makeReservation, deleteReservation, moveReservation } = useReservation()
  const [ freeze, setFreeze ] = useState<boolean>(false)

  async function onSeatClick(seat: Seat) {
    if (freeze) return
    
    if (!loggedInUser) {
      alertWarning("Du må være logget inn for å reservere et sete")
    } else if (seat.reservedBy && seat.reservedBy.id !== loggedInUser.id) {
      alertWarning("Plass " + seat.title + " er opptatt");
    } else if (seat.reservedBy && seat.reservedBy.id === loggedInUser.id) {
      showDialog({
        title: "Fjern reservasjon",
        description: "Sikker på at du vil gi fra deg denne plassen?",
        metadata: seat,
        positiveText: "Ja",
        negativeText: "Nei",
        positiveCallback: async (seat) => {
          setFreeze(true)
          await alertLoading("Sletter reservasjon...", async () => {
            await deleteReservation(seat)
            await reloadSeats()
          })

          setFreeze(false)
          alertInfo("Du har slettet din reservasjon")
         }
      })
    } else if (reservedSeat) {
      showDialog({
        title: "Endre sete",
        description: "Sikker på at du vil endre sete fra " + reservedSeat.title + " til " + seat.title + "?",
        metadata: seat,
        positiveText: "Ja",
        negativeText: "Nei",
        positiveCallback: async (toSeat) => {
          setFreeze(true)
          let fromSeat = reservedSeat
          await alertLoading("Flytter reservasjon...", async () => {
            await moveReservation(fromSeat, toSeat)
            await reloadSeats()
          })

          setFreeze(false)
          alertInfo("Du har flyttet din reservasjon fra sete " + fromSeat.title + " til sete " + toSeat.title)
        }
      })
    } else {
      setFreeze(true)
      await alertLoading("Reserverer...", async () => {
        await makeReservation(seat)
        await reloadSeats()
      })

      setFreeze(false)
      alertInfo("Du har reservert sete " + seat.title)
    }
  }

  const getSeatColor = (seat: Seat): string => {
    if (seat.reservedBy && loggedInUser && seat.reservedBy.id == loggedInUser.id) {
      return "#0f3f6a"
    } else if (seat.reservedBy) {
      return "#aa3030"
    } else {
      return "#0f6a0f"
    }
  }

  return (
    <Stack sx={{ my: 2, alignItems: 'center' }}>
      <Box sx={{ flexGrow: 1 }}>
        <Box sx={{
          display: "flex",
          width: "100%",
          height: "calc(100% - 64px)",
          overflowX: "hidden",
        }}>
          <Box sx={{
            marginBottom: "auto",
            marginLeft: "auto",
            marginRight: "auto",
            position: "relative"
          }}>
            <img src={background} alt="" style={{
              width: "100%"
            }}/>

            {seats && seats.map(seat => <SeatComponent key={seat.id} color={getSeatColor(seat)} seat={seat} onClick={onSeatClick}/>)}
          </Box>
        </Box>
      </Box>
    </Stack>
  )
}
