import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr"
import { atomFamily, DefaultValue, useRecoilState } from "recoil"
import Config from "../config"
import { ApiRequest } from "./ApiRequest"
import { useAuth } from "./AuthAdapter"
import { Lan, Seat, User } from "./Models"

async function LoadSeats(guildId: string, lanId: string) {
  const response = await ApiRequest("GET", `guild/${guildId}/lan/${lanId}/seat`)
  const seats = (await response.json()) as Seat[]

  return seats.reduce((map, seat) => map.set(seat.id, seat), new Map<string, Seat>())
}

const seatsAtomFamily = atomFamily<Map<string, Seat>, [string, string]>({
  key: "seats",
  default: ([guildId, lanId]) => LoadSeats(guildId, lanId),
  effects: ([, lanId]) => [
    ({ setSelf }) => {
      const connection = new HubConnectionBuilder()
        .withUrl(Config.ApiBaseUrl + `/hubs/reservation?lanId=${lanId}`)
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
        setSelf(seats => {
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

export function useSeats(guildId: string, lan: Lan) {
  const [seatsMap, setSeatsMap] = useRecoilState(seatsAtomFamily([guildId, lan.id]))
  const { loggedInUser } = useAuth()
  const seats = Array.from(seatsMap.values())
  const reservedSeat = findSeatReservedBy(seats, loggedInUser)

  const createNewSeat = async (seat: Seat) => {
    await ApiRequest("POST", `guild/${guildId}/lan/${lan.id}/seat`, {
      title: seat.title,
      bounds: seat.bounds,
    })
    setSeatsMap(await LoadSeats(guildId, lan.id))
  }

  return { seats, reservedSeat, createNewSeat }
}
