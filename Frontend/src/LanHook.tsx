import {useEffect, useState} from "react";
import Lan from "./Models/Lan";
import useApiRequests from "./ApiRequestHook";

export default function useLans() {
  const { apiRequest, apiRequestJson } = useApiRequests()
  const [ lans, setLans ] = useState<Lan[]>([]);
  const [ isLoading, setIsLoading ] = useState<boolean>(true)
  
  useEffect(() => {
    reloadLans()
  }, [])

  const createNewLan = async (title : string, background : string) : Promise<Response> =>  {
    return apiRequest("POST", `lan`, { title: title, background: background })
  }

  const updateLan = (lan: Lan, title: string, background: string) : Promise<Response> =>  {
    return apiRequest("PUT", `lan/${lan.id}`, { id: lan.id, title: title, background: background })
  }
  
  const deleteLan = (lan: Lan) : Promise<Response> =>  {
    return apiRequest("DELETE", `lan/${lan.id}`)
  }

  const reloadLans = async () : Promise<Lan[]> =>  {
    setIsLoading(true)
    let lans = await apiRequestJson<Lan[]>("GET", `lan`)
    lans.forEach(lan => {
      lan.createdAt = new Date(lan.createdAt)
      lan.updatedAt = new Date(lan.createdAt)
    })
    setLans(lans)
    setIsLoading(false)
    return lans;
  }

  return { createNewLan, lans, reloadLans, deleteLan, updateLan, isLoading }
}