import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr"
import { DefaultValue, atomFamily, useRecoilState } from "recoil"
import Config from "../config"
import ApiRequest from "./ApiRequest"
import { useAuth } from "./AuthAdapter"
import { Lan, Seat, User } from "./Models"

async function LoadSeats(lanId: string) {
  const response = await ApiRequest("GET", `lan/${lanId}/seat`)
  const seats = (await response.json()) as Seat[]

  return seats.reduce((map, seat) => map.set(seat.id, seat), new Map<string, Seat>())
}

const seatsAtomFamily = atomFamily<Map<string, Seat>, string>({
  key: "seats",
  default: (lanId) => LoadSeats(lanId),
  effects: () => [
    ({ setSelf }) => {
      const connection = new HubConnectionBuilder()
        .withUrl(Config.ApiBaseUrl + "/hubs/reservation")
        .configureLogging(LogLevel.Information)
        .build()

      async function start() {
        try {
          await connection.start()
        } catch (err) {
          console.log(err)
          setTimeout(start, 3000)
        }
      }

      connection.on("ReservationChanged", (update: { id: string; reservedBy: User | null }) =>
        setSelf((seats) => {
          if (seats instanceof DefaultValue) return seats
          const seat = seats.get(update.id)
          if (!seat) return seats

          const copy = new Map(seats)
          copy.set(update.id, { ...seat, reservedBy: update.reservedBy })

          return copy
        })
      )

      start()

      return () => {
        connection.stop()
      }
    },
  ],
})

function findSeatReservedBy(seats: Seat[], loggedInUser: User | null) {
  if (loggedInUser) {
    for (let i = 0; i < seats.length; i++) {
      const seat = seats[i]
      if (seat.reservedBy != null && seat.reservedBy.id == loggedInUser.id) {
        return seat
      }
    }
  }
  return null
}

export function useSeats(lan: Lan) {
  const [seatsMap, setSeatsMap] = useRecoilState(seatsAtomFamily(lan.id))
  const { loggedInUser } = useAuth()
  const seats = Array.from(seatsMap.values())
  const reservedSeat = findSeatReservedBy(seats, loggedInUser)

  const createNewSeat = async (seat: Seat) => {
    await ApiRequest("POST", `lan/${lan.id}/seat`, {
      title: seat.title,
      bounds: seat.bounds,
    })
    setSeatsMap(await LoadSeats(lan.id))
  }

  return { seats, reservedSeat, createNewSeat }
}
