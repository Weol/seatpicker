import { Stack } from "@mui/material"
import DelayedCircularProgress from "../Components/DelayedCircularProgress"

export default function Loading() {
  return (
    <Stack width="100%" justifyContent="center" alignItems="center" sx={{ marginTop: "1em" }}>
      <DelayedCircularProgress />
    </Stack>
  )
}
