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
import {useAlerts} from "../AlertContext";
import Button from "@mui/material/Button";
import SeatComponent from '../Components/SeatComponent';
import Seat from '../Models/Seat';
import createSeats from "../StaticSeats";
import useSeats from "../SeatsHook"
import useReservation from "../ReservationHook"
import useLoggedInUser from "../LoggedInUserHook";

interface DialogModel<T> {
  title: string;
  description: string;
  positiveText: string;
  negativeText: string;
  positiveCallback?: (data: T) => void;
  negativeCallback?: (data: T) => void;
  metadata: T;
}

var lmao = createSeats()

export default function Seats() {
  const [ dialog, setDialog ] = useState<DialogModel<any> | null>(null)
  const { setAlert } = useAlerts()
  const loggedInUser = useLoggedInUser()
  const { seats, reloadSeats, reservedSeat, setSeatReservedBy, createNewSeat } = useSeats()
  const { makeReservation, deleteReservation, moveReservation } = useReservation()

  useEffect(() => {
    lmao.forEach(seat => {
      // createNewSeat(seat)
    })
  }, []);

  async function onSeatClick(seat: Seat) {
    if (!loggedInUser) {
      setAlert({
        type: "warning",
        title: "Du må være logget inn for å reservere et sete"
      })
    } else if (seat.reservedBy && seat.reservedBy.id !== loggedInUser.id) {
      setAlert({
        type: "warning",
        title: "Plass " + seat.title + " er opptatt",
      })
    } else if (seat.reservedBy && seat.reservedBy.id === loggedInUser.id) {
      setDialog({
        title: "Fjern reservasjon",
        description: "Sikker på at du vil gi fra deg denne plassen?",
        metadata: seat,
        positiveText: "Ja",
        negativeText: "Nei",
        positiveCallback: async (seat) => {
          deleteReservation(seat).then(() => {
            setSeatReservedBy(seat, null)
            reloadSeats()
          })
        }
      })
    } else if (reservedSeat) {
      setDialog({
        title: "Endre sete",
        description: "Sikker på at du vil endre sete fra " + reservedSeat.title + " til " + seat.title + "?",
        metadata: seat,
        positiveText: "Ja",
        negativeText: "Nei",
        positiveCallback: async (seat) => {
          moveReservation(reservedSeat, seat).then(() => {
            setSeatReservedBy(reservedSeat, null)
            setSeatReservedBy(seat, loggedInUser)
            reloadSeats()
          })
        }
      })
    } else {
      makeReservation(seat)
      .then((response) => {
        setSeatReservedBy(seat, loggedInUser)
        reloadSeats()
      })
      setAlert({
        type: "success",
        title: "Du har reservert sete " + seat.title,
      })
    }
  }

  const renderDialog = () =>
    (dialog &&
        <Dialog
            open={true}
            onClose={() => setDialog(null)}
            aria-labelledby="alert-dialog-title"
            aria-describedby="alert-dialog-description"
        >
            <DialogTitle id="alert-dialog-title">
              {dialog.title}
            </DialogTitle>
            <DialogContent>
                <DialogContentText id="alert-dialog-description">
                  {dialog.description}
                </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button onClick={() => {
                  setDialog(null)
                  if (dialog.negativeCallback) dialog.negativeCallback(dialog.metadata);
                }}>{dialog.negativeText}</Button>
                <Button onClick={() => {
                  setDialog(null)
                  if (dialog.positiveCallback) dialog.positiveCallback(dialog.metadata);
                }}>{dialog.positiveText}</Button>
            </DialogActions>
        </Dialog>
    )

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
      {renderDialog()}
    </Stack>
  )
}
