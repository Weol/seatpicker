import * as React from "react"
import { useState } from "react"
import { Box, Stack, Typography } from "@mui/material"
import { useAlerts } from "../Contexts/AlertContext"
import SeatComponent from "../Components/SeatComponent"
import useReservationAdapter from "../Adapters/ReservationAdapter"
import { useDialogs } from "../Contexts/DialogContext"
import { useAuthenticationAdapter } from "../Adapters/AuthenticationAdapter"
import { Seat, useSeats } from "../Adapters/SeatsAdapter"
import { Lan, useActiveLan } from "../Adapters/LanAdapter"

export default function Seats() {
  const activeLan = useActiveLan()

  return activeLan ? <SeatsWithLan activeLan={activeLan} /> : <NoActiveLan />
}

function NoActiveLan() {
  return <Typography>No active lan is configured</Typography>
}

function SeatsWithLan(props: { activeLan: Lan }) {
  const { alertWarning, alertInfo, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const { loggedInUser } = useAuthenticationAdapter()
  const { seats, reservedSeat } = useSeats(props.activeLan)
  const { makeReservation, deleteReservation, moveReservation } =
    useReservationAdapter(props.activeLan)
  const [freeze, setFreeze] = useState<boolean>(false)

  async function onSeatClick(seat: Seat) {
    if (freeze) return

    if (loggedInUser == null) {
      alertWarning("Du må være logget inn for å reservere et sete")
    } else if (
      seat.reservedBy != null &&
      seat.reservedBy.id !== loggedInUser.id
    ) {
      alertWarning("Plass " + seat.title + " er opptatt")
    } else if (
      seat.reservedBy != null &&
      seat.reservedBy.id === loggedInUser.id
    ) {
      const result = await showDialog(
        "Fjern reservasjon",
        "Sikker på at du vil gi fra deg denne plassen?",
        seat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        setFreeze(true)
        await alertLoading("Sletter reservasjon...", async () => {
          await deleteReservation(result.metadata)
        })

        setFreeze(false)
        alertInfo("Du har slettet din reservasjon")
      }
    } else if (reservedSeat != null) {
      const result = await showDialog(
        "Endre sete",
        `Sikker på at du vil endre sete fra ${reservedSeat.title} til ${seat.title}?`,
        seat,
        "Ja",
        "Nei"
      )

      if (result.positive) {
        const toSeat = result.metadata

        setFreeze(true)
        const fromSeat = reservedSeat
        await alertLoading("Flytter reservasjon...", async () => {
          await moveReservation(fromSeat, toSeat)
        })

        setFreeze(false)
        alertInfo(
          `Du har flyttet din reservasjon fra sete ${fromSeat.title} til sete ${toSeat.title}`
        )
      }
    } else {
      setFreeze(true)
      await alertLoading("Reserverer...", async () => {
        await makeReservation(seat)
      })

      setFreeze(false)
      alertInfo("Du har reservert sete " + seat.title)
    }
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
    <Stack sx={{ my: 2, alignItems: "center" }}>
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
                onClick={onSeatClick}
              />
            ))}
          </Box>
        </Box>
      </Box>
    </Stack>
  )
}
