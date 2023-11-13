import * as React from "react"
import { useState } from "react"
import { Box, Stack } from "@mui/material"
import { useAlerts } from "../Contexts/AlertContext"
import SeatComponent from "../Components/SeatComponent"
import useSeats, { Seat } from "../Adapters/SeatsAdapter"
import useReservationAdapter from "../Adapters/ReservationAdapter"
import { useDialogs } from "../Contexts/DialogContext"
import { useAuthenticationAdapter } from "../Adapters/AuthenticationAdapter"

// eslint-disable-next-line @typescript-eslint/no-var-requires
const background = require("../Media/background.svg").default

export default function Seats() {
  const { alertWarning, alertInfo, alertLoading } = useAlerts()
  const { showDialog } = useDialogs()
  const { loggedInUser } = useAuthenticationAdapter()
  const { seats, reloadSeats, reservedSeat } = useSeats()
  const { makeReservation, deleteReservation, moveReservation } =
    useReservationAdapter()
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
          await reloadSeats()
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
          await reloadSeats()
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
        await reloadSeats()
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
              src={background}
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
