import type { FC } from 'react'
import { KitchenProvider } from './contexts/KitchenContext'
import KitchenScreen from './components/KitchenScreen'

const App: FC = () => {
  return (
    <KitchenProvider>
      <KitchenScreen />
    </KitchenProvider>
  )
}

export default App
