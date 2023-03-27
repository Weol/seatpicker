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
import Seat from '../Models/Seat';
import CreateReservation from "../Adapters/CreateReservation";
import GetAllSeats from "../Adapters/GetAllSeats";
import {useUserContext} from "../UserContext";
import DeleteReservation from '../Adapters/DeleteReservation';
import {useAlertContext} from "../AlertContext";
import Button from "@mui/material/Button";
import ReplaceReservation from '../Adapters/ReplaceReservation';
import SeatComponent from '../Components/Seat';
import StaticSeats from '../StaticSeats';

interface DialogModel<T> {
  title: string;
  description: string;
  positiveText: string;
  negativeText: string;
  positiveCallback?: (data: T) => void;
  negativeCallback?: (data: T) => void;
  metadata: T;
}

export default function Seats() {
  const [seats, setSeats] = useState<Seat[]>([])
  const [selectedSeat, setSelectedSeat] = useState<Seat | null>(null)
  const [dialog, setDialog] = useState<DialogModel<any> | null>(null)
  const {user} = useUserContext()
  const {setAlert} = useAlertContext()

  useEffect(() => {
    const interval = setInterval(() => {
      fetchAllSeats()
    }, 5000);

    return () => clearInterval(interval);
  }, [])

  const fetchAllSeats = () => {
    GetAllSeats().then(seats => {
      setSeats(seats)

      setSelectedSeat(null)
      for (let seat of seats) {
        if (seat.user && seat.user.id == user?.id) {
          setSelectedSeat(seat);
          break;
        }
      }
    })
  }

  async function onSeatClick(seat: Seat) {
    if (!user) {
      setAlert({
        type: "warning",
        title: "Du må være logget inn for å reservere et sete",
        description: ""
      })
    } else if (seat.user && seat.user.id !== user.id) {
      setAlert({
        type: "warning",
        title: "Plass " + seat.title + " er opptatt",
        description: ""
      })
    } else if (seat.user && seat.user.id === user.id) {
      setDialog({
        title: "Fjern reservasjon",
        description: "Sikker på at du vil gi fra deg denne plassen?",
        metadata: seat.id,
        positiveText: "Ja",
        negativeText: "Nei",
        positiveCallback: async (seatId) => {
          await DeleteReservation(seatId)
          fetchAllSeats()
        }
      })
    } else if (selectedSeat) {
      setDialog({
        title: "Endre sete",
        description: "Sikker på at du vil endre sete fra " + selectedSeat.title + " til " + seat.title + "?",
        metadata: seat.id,
        positiveText: "Ja",
        negativeText: "Nei",
        positiveCallback: async (seatId) => {
          await ReplaceReservation(selectedSeat.id, seatId)
          fetchAllSeats()
        }
      })
    } else {
      await CreateReservation(seat.id)
      fetchAllSeats()
      setAlert({
        type: "success",
        title: "Du har reservert sete " + seat.title,
        description: ""
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
    if (seat.user && user && seat.user.id === user.id) {
      return "#0f3f6a"
    } else if (seat.user) {
      return "#aa3030"
    } else {
      return "#0f6a0f"
    }
  }

  return (
    <Stack sx={{my: 2, alignItems: 'center'}}>
      <Box sx={{flexGrow: 1}}>
        <Box sx={{
          display: "flex",
          width: "100%",
          height: "calc(100% - 64px)",
          overflowX: "hidden",
        }}>
          <Box sx={{
            marginTop: "1em",
            marginBottom: "auto",
            marginLeft: "auto",
            marginRight: "auto",
            position: "relative"
          }}>
            <img src={background} alt="" style={{
              width: "100%"
            }}/>

            {seats && seats.map(seat => <SeatComponent color={getSeatColor(seat)} seat={seat} onClick={onSeatClick} />)}
          </Box>
        </Box>
      </Box>
      {renderDialog()}
    </Stack>
  )
}
