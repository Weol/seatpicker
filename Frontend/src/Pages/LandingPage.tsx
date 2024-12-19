import {Button, Stack} from "@mui/material"
import Typography from "@mui/material/Typography"
import React from "react";


export default function LandingPage() {
  return (
    <Stack sx={{ alignItems: "center" }}>
      <Typography variant="h1" component="h1" gutterBottom>
        SaltenLAN
      </Typography>
      <Typography variant="h4" component="h1" gutterBottom>
       17. til 19. januar 
      </Typography>
      <Button variant={"contained"}>Resever en plass</Button>
    </Stack>
  )
}
