import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr"
import { useEffect, useRef, useState } from "react"
import Config from "../config"
import useApiRequests from "./ApiRequestHook"
import { User, useAuthenticationAdapter } from "./AuthenticationAdapter"
import { Lan } from "./LanAdapter"
import useVersionId from "./VersionIdHook"

export interface Bounds {
  x: number
  y: number
  width: number
  height: number
}

export interface Seat {
  id: string
  title: string
  bounds: Bounds
  reservedBy: User | null
}

export function useSeats(lan: Lan | null) {
  const { apiRequest } = useApiRequests()
  const { loggedInUser } = useAuthenticationAdapter()
  const { versionId } = useVersionId("Seats")
  const [seats, setSeats] = useState<Seat[] | null>(null)
  const reservedSeat = findSeatReservedBy(seats, loggedInUser)
  const connectionRef = useRef<HubConnection | null>(null)

  function findSeatReservedBy(seats: Seat[] | null, loggedInUser: User | null) {
    if (seats && loggedInUser) {
      for (let i = 0; i < seats.length; i++) {
        const seat = seats[i]
        if (seat.reservedBy != null && seat.reservedBy.id == loggedInUser.id) {
          return seat
        }
      }
    }
    return null
  }

  useEffect(() => {
    loadSeats()
  }, [lan?.id, versionId])

  useEffect(() => {
    if (connectionRef.current == null && seats) {
      subscribeToUpdates()
    }

    if (connectionRef.current != null) {
      connectionRef.current.on("ReservationChanged", onReservationChanged)
    }

    return unsubscribeFromUpdates
  }, [seats])

  function onReservationChanged(update: { id: string; reservedBy: User | null }) {
    setSeats((seats) => {
      return (
        (seats &&
          seats.map((seat) => {
            return seat.id == update.id ? { ...seat, reservedBy: update.reservedBy } : seat
          })) ??
        []
      )
    })
  }

  function unsubscribeFromUpdates() {
    if (connectionRef.current != null) {
      connectionRef.current.stop()
      connectionRef.current = null
    }
  }

  function subscribeToUpdates() {
    const connection = new HubConnectionBuilder()
      .withUrl(Config.ApiBaseUrl + "/hubs/reservation")
      .configureLogging(LogLevel.Information)
      .build()

    async function start() {
      try {
        await connection.start()
      } catch (err) {
        console.log(err)
        setTimeout(start, 1000)
      }
    }

    start()

    connectionRef.current = connection
  }

  async function loadSeats() {
    const response = await apiRequest("GET", `lan/${lan?.id}/seat`)

    const seats = (await response.json()) as Seat[]
    setSeats(seats)
  }

  return { seats, reservedSeat }
}

export function useSeatAdapter(lan: Lan) {
  const { apiRequest } = useApiRequests()

  const createNewSeat = (seat: Seat) => {
    return apiRequest("POST", `lan/${lan.id}/seat`, {
      title: seat.title,
      bounds: seat.bounds,
    })
  }

  return { createNewSeat }
}
