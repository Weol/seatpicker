import CircularProgress from "@mui/material/CircularProgress"
import { CircularProgressProps } from "@mui/material/CircularProgress/CircularProgress"
import Container from "@mui/material/Container"
import { useEffect, useState } from "react"

export default function DelayedCircularProgress(params: CircularProgressProps) {
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    setTimeout(() => {
      setVisible(true)
    }, 1000)
  }, [])

  return visible ? <CircularProgress {...params} /> : <Container />
}
