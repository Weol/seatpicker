import * as React from "react"
import { Box, Container, Typography } from "@mui/material"
import { SeatMenu, SeatMenuProps } from "./SeatMenu"
import { Seat } from "../Adapters/SeatsAdapter"

type SeatProperties = {
  color: string
  onSeatClick?: (seat: Seat) => void
} & SeatMenuProps

export default function SeatComponent(props: SeatProperties) {
  const [menuAnchor, setAnchor] = React.useState<null | HTMLElement>(null)
  const menuOpen = Boolean(menuAnchor)

  const handleClick = (event: React.MouseEvent<HTMLElement>) => {
    if (props.onSeatClick != undefined) {
      props.onSeatClick(props.seat)
    } else {
      setAnchor(event.currentTarget)
    }
  }

  const handleClose = () => {
    setAnchor(null)
  }

  function handleRemove(seat: Seat) {
    props.onRemove(seat)
    handleClose()
  }

  function handleRemoveFor(seat: Seat) {
    props.onRemoveFor(seat)
    handleClose()
  }

  function handleMoveFor(seat: Seat) {
    props.onMoveFor(seat)
    handleClose()
  }

  const renderSeat = (seat: Seat) => {
    return (
      <Container>
        <Box
          key={seat.id}
          onClick={handleClick}
          sx={{
            position: "absolute",
            minWidth: "0",
            top: seat.bounds.y + "%",
            left: seat.bounds.x + "%",
            width: seat.bounds.width + "%",
            height: seat.bounds.height + "%",
            border: "1px #ffffff61 solid",
            backgroundColor: props.color,
            cursor: "pointer",
            textAlign: "center",
            display: "flex",
          }}
        >
          <Typography
            variant="subtitle1"
            gutterBottom
            component="p"
            sx={{ lineHeight: 1, fontSize: "0.9rem", margin: "auto" }}
          >
            {seat.title}
          </Typography>
        </Box>
        <SeatMenu
          {...props}
          onRemove={handleRemove}
          onRemoveFor={handleRemoveFor}
          onMoveFor={handleMoveFor}
          open={menuOpen}
          seat={seat}
          id="demo-positioned-menu"
          aria-labelledby="demo-positioned-button"
          anchorEl={menuAnchor}
          onClose={handleClose}
          anchorOrigin={{
            vertical: "top",
            horizontal: "center",
          }}
          transformOrigin={{
            vertical: "bottom",
            horizontal: "center",
          }}
        />
      </Container>
    )
  }

  return renderSeat(props.seat)
}
