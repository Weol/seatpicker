/* eslint-disable @typescript-eslint/no-unused-vars */
import { Seat } from "./Adapters/Models"

interface SeatReference {
  Id: string | null
  Title: string | null
  Width: number
  Height: number
  Y: number
  X: number
}

const createColumn = (
  width: number,
  height: number,
  x: number,
  y: number,
  n: number,
  xOffset: number,
  yOffset: number
): SeatReference[] => {
  const row = []
  for (let i = 0; i < n; i++) {
    row.push({
      Id: null,
      Title: null,
      Y: y + yOffset * i,
      X: x + xOffset * i,
      Width: width,
      Height: height,
    })
  }
  return row
}

export default function createSeats(): Seat[] {
  const tableWidth = 8.4
  const tableHeight = 6.3

  const xOffset = tableWidth + 0.52

  const x = 27.7
  const y = 30.6

  const col1 = createColumn(tableWidth, tableHeight, x, y, 4, 0, tableHeight)
  const col2 = createColumn(tableWidth, tableHeight, x + xOffset * 2, y, 6, 0, tableHeight)
  const col3 = createColumn(tableWidth, tableHeight, x + xOffset * 4, y, 6, 0, tableHeight)
  const col4 = createColumn(
    tableWidth,
    tableHeight,
    x + xOffset * 4 + tableWidth,
    y,
    6,
    0,
    tableHeight
  )
  const col5 = createColumn(
    tableWidth,
    tableHeight,
    x + xOffset * 7,
    y + tableHeight - 0.85,
    6,
    0,
    tableHeight
  )

  const seats = [...col1, ...col2, ...col3, ...col4, ...col5]

  let title = 0
  return seats.map((seat) => {
    title++
    return {
      id: crypto.randomUUID(),
      title: title + "",
      reservedBy: null,
      bounds: {
        width: seat.Width,
        height: seat.Height,
        y: seat.Y,
        x: seat.X,
      },
    }
  })
}
