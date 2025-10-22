import { FC } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { CartProvider } from './contexts/CartContext'
import HomeScreen from './screens/HomeScreen'
import MenuListScreen from './screens/MenuListScreen'
import ItemDetailScreen from './screens/ItemDetailScreen'
import CartScreen from './screens/CartScreen'
import OrderStatusScreen from './screens/OrderStatusScreen'
import './App.css'

const App: FC = () => {
  return (
    <CartProvider>
      <Routes>
        <Route path="/" element={<HomeScreen />} />
        <Route path="/menu/:category" element={<MenuListScreen />} />
        <Route path="/menu/item/:id" element={<ItemDetailScreen />} />
        <Route path="/cart" element={<CartScreen />} />
        <Route path="/orders" element={<OrderStatusScreen />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </CartProvider>
  )
}

export default App
