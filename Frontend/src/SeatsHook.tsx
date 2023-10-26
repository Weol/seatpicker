import {useAppState} from "./AppStateContext";
import {useEffect, useState} from "react";
import Seat from "./Models/Seat";
import ApiRequestJson, {ApiRequest} from "./Adapters/ApiRequest";

export default function useSeats() {
  const { appState } = useAppState();
  const [ seats, setSeats ] = useState<Seat[]>([]);

  useEffect(() => {
    reloadSeats()
  }, [ appState.activeLan ]);

  const reloadSeats = () => {
    ApiRequestJson<Seat[]>("GET", `lan/${appState.activeLan}/seat`, null)
    .then(seats => {
      setSeats(seats)
    });
  }

  return { seats, reloadSeats }
}