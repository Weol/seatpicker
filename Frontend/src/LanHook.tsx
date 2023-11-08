import {useAppState} from "./AppStateContext";
import {useEffect, useState} from "react";
import Seat from "./Models/Seat";
import ApiRequestJson, {ApiRequest} from "./Adapters/ApiRequest";
import User from "./Models/User";
import Lan from "./Models/Lan";

export default function useLans() {
  const { appState } = useAppState();
  const [ lans, setLans ] = useState<Lan[]>([]);

  useEffect(() => {
    reloadLans()
  }, [])

  const createNewLan = (title : string, background : string) : Promise<Response> =>  {
    return ApiRequest("POST", `lan`, appState.authenticationToken ? appState.authenticationToken.token : null, { title: title, background: background })
  }

  const deleteLan = (lan: Lan) : Promise<Response> =>  {
    return ApiRequest("DELETE", `lan/${lan.id}`, appState.authenticationToken ? appState.authenticationToken.token : null)
  }

  const reloadLans = () =>  {
    ApiRequestJson<Lan[]>("GET", `lan`, appState.authenticationToken ? appState.authenticationToken.token : null)
      .then((lans) => {
        setLans(lans)
      })
  }

  return { createNewLan, lans, reloadLans, deleteLan }
}