import { Component, ReactNode } from "react"

type Props = {
  children: ReactNode
  fallback: ((error: unknown) => ReactNode) | ReactNode
  catch?: (error: unknown) => boolean
}

class ErrorBoundary2 extends Component<Props, { error: unknown | null }> {
  constructor(props: Props) {
    super(props)
    this.state = { error: null }
  }

  static getDerivedStateFromError(error: unknown) {
    return { error: error }
  }

  renderFallback = () =>
    typeof this.props.fallback == "function"
      ? this.props.fallback(this.state.error)
      : this.props.fallback

  render() {
    if (this.state.error != null) {
      if (this.props.catch) {
        if (this.props.fallback && this.props.catch(this.state.error)) {
          return this.renderFallback()
        }

        throw this.state.error
      }
      return this.renderFallback()
    }

    return <div>{this.props.children}</div>
  }
}

export default ErrorBoundary2
