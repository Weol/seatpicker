/* eslint-disable @typescript-eslint/no-unused-vars */
import * as React from "react"
import { useState } from "react"
import {
  Box,
  Button,
  Card,
  CardActions,
  CardContent,
  Stack,
  Typography,
} from "@mui/material"
import { useAlerts } from "../Contexts/AlertContext"
import SeatComponent from "../Components/SeatComponent"
import useReservationAdapter from "../Adapters/ReservationAdapter"
import { useDialogs } from "../Contexts/DialogContext"
import {
  User,
  useAuthenticationAdapter,
} from "../Adapters/AuthenticationAdapter"
import { Seat, useSeats } from "../Adapters/SeatsAdapter"
import { Lan, useActiveLan } from "../Adapters/LanAdapter"
import DelayedCircularProgress from "../Components/DelayedCircularProgress"

export default function Seats() {
  const activeLan = useActiveLan()

  return activeLan ? <SeatsWithLan activeLan={activeLan} /> : <NoActiveLan />
}

function NoActiveLan() {
  return (
    <Stack
      width="100%"
      justifyContent="center"
      alignItems="center"
      sx={{ marginTop: "1em" }}
    >
      <Typography>No active lan is configured</Typography>
    </Stack>
  )
}

type AwaitingSelectSeat = {
  onSelected: (seat: Seat | null) => Promise<void>
  seat: Seat
  tooltip: string
}

function SeatsWithLan(props: { activeLan: Lan }) {
  const { alertInfo, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const { loggedInUser } = useAuthenticationAdapter()
  const { seats, reservedSeat } = useSeats(props.activeLan)
  const {
    makeReservation,
    deleteReservation,
    deleteReservationFor,
    moveReservation,
    moveReservationFor,
  } = useReservationAdapter(props.activeLan)
  const [awaitingSelectSeat, setAwaitingSelectSeat] =
    useState<AwaitingSelectSeat | null>(null)

  async function handleReserve(toSeat: Seat) {
    if (reservedSeat != null) {
      const fromSeat = reservedSeat
      const result = await showDialog(
        "Endre sete",
        `Sikker på at du vil endre sete fra ${fromSeat.title} til ${toSeat.title}?`,
        toSeat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        const toSeat = result.metadata

        await alertLoading("Flytter reservasjon...", async () => {
          await moveReservation(fromSeat, toSeat)
        })

        alertInfo(
          `Du har flyttet din reservasjon fra plass ${fromSeat.title} til plass ${toSeat.title}`
        )
      }
    } else {
      await alertLoading("Reserverer...", async () => {
        await makeReservation(toSeat)
      })

      alertInfo("Du har reservert plass " + toSeat.title)
    }
  }

  async function handleRemove(seat: Seat) {
    const result = await showDialog(
      "Fjern reservasjon",
      "Sikker på at du vil gi fra deg denne plassen?",
      seat,
      "Ja",
      "Nei"
    )

    if (result.positive) {
      await alertLoading("Sletter reservasjon...", async () => {
        await deleteReservation(result.metadata)
      })

      alertInfo("Du har slettet din reservasjon")
    }
  }

  async function handleRemoveFor(seat: Seat) {
    if (seat.reservedBy) {
      const result = await showDialog(
        "Fjern reservasjon",
        `Sikker på at du vil fjerne ${seat.reservedBy.name} fra plass ${seat.title}?`,
        seat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        await alertLoading("Fjerner reservasjon...", async () => {
          await deleteReservationFor(result.metadata)
        })

        alertInfo(`Du har fjernet ${seat.reservedBy.name} sin reservasjon`)
      }
    }
  }

  function handleReserveFor(seat: Seat) {
    throw new Error("Function not implemented.")
  }

  async function onSeatSelectedForMove(fromSeat: Seat, toSeat: Seat | null) {
    if (toSeat) {
      const result = await showDialog(
        "Endre sete",
        `Sikker på at du vil flytte ${fromSeat.reservedBy?.name} fra plass ${fromSeat.title} til plass ${toSeat.title}?`,
        toSeat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        const toSeat = result.metadata

        await alertLoading("Flytter reservasjon...", async () => {
          await moveReservationFor(fromSeat, toSeat)
        })

        alertInfo(
          `Du har flyttet ${fromSeat.reservedBy?.name} fra plass ${fromSeat.title} til plass ${toSeat.title}`
        )
      }
    }
  }

  function handleMoveFor(fromSeat: Seat) {
    // This looks kind of wierd, but in order to set the state to a function we need to give the
    // setter a function that returns a function, google it
    setAwaitingSelectSeat({
      seat: fromSeat,
      tooltip: `Velg hvilken plass du vil flytte ${fromSeat.reservedBy?.name} til`,
      onSelected: async (toSeat: Seat | null) => {
        await onSeatSelectedForMove(fromSeat, toSeat)
      },
    })
  }

  async function handleSeatClick(seat: Seat) {
    if (awaitingSelectSeat) {
      await awaitingSelectSeat.onSelected(seat)
      setAwaitingSelectSeat(null)
    }
  }

  function handleCancelMove() {
    setAwaitingSelectSeat(null)
  }

  const getSeatColor = (seat: Seat): string => {
    if (
      seat.reservedBy != null &&
      loggedInUser != null &&
      seat.reservedBy.id === loggedInUser.id
    ) {
      return "#0f3f6a"
    } else if (seat.reservedBy != null) {
      return "#aa3030"
    } else {
      return "#0f6a0f"
    }
  }

  return (
    <Stack spacing={2} sx={{ my: 2, alignItems: "center" }}>
      {awaitingSelectSeat && (
        <Card>
          <CardContent>
            <Typography color="text.secondary" gutterBottom>
              Du flytter på {awaitingSelectSeat.seat.reservedBy?.name} fra plass{" "}
              {awaitingSelectSeat.seat.title}
            </Typography>
            <Typography>
              Trykk på en plass for å velge hvor du vil flytte{" "}
              {awaitingSelectSeat.seat.reservedBy?.name} til
            </Typography>
          </CardContent>
          <CardActions>
            <Button onClick={handleCancelMove} size="small">
              Avbryt
            </Button>
          </CardActions>
        </Card>
      )}
      <Box sx={{ flexGrow: 1 }}>
        <Box
          sx={{
            display: "flex",
            width: "100%",
            height: "calc(100% - 64px)",
            overflowX: "hidden",
          }}
        >
          <Box
            sx={{
              marginBottom: "auto",
              marginLeft: "auto",
              marginRight: "auto",
              position: "relative",
            }}
          >
            <img
              src={`data:image/svg+xml;base64,${props.activeLan.background}`}
              alt=""
              style={{
                width: "100%",
              }}
            />

            {seats?.map((seat) => (
              <SeatComponent
                key={seat.id}
                color={getSeatColor(seat)}
                seat={seat}
                onSeatClick={awaitingSelectSeat ? handleSeatClick : undefined}
                onReserve={handleReserve}
                onRemove={handleRemove}
                onReserveFor={handleReserveFor}
                onRemoveFor={handleRemoveFor}
                onMoveFor={handleMoveFor}
              />
            ))}
            {!seats && (
              <Stack width="100%" justifyContent="center" alignItems="center">
                <DelayedCircularProgress />
              </Stack>
            )}
          </Box>
        </Box>
      </Box>
    </Stack>
  )
}
