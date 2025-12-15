using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Wpf;
using Check_IT.Interfaces;

namespace Check_IT.Views
{
    public partial class HomeView : UserControl
    {
        private readonly IServiceProvider? _serviceProvider;
        private readonly IAuthService? _authService;

        public HomeView()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            _serviceProvider = App.AppHost?.Services;
            _authService = _serviceProvider?.GetService<IAuthService>();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await HeaderWebView.EnsureCoreWebView2Async(null);

                string htmlContent = @"
<!DOCTYPE html>
<html lang='uk'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Check IT - GSAP Анімація</title>
    <script src='https://cdnjs.cloudflare.com/ajax/libs/gsap/3.12.2/gsap.min.js'></script>
    <style>
        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            background: #ffffff;
            height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            overflow: hidden;
            position: relative;
            cursor: default;
        }

        .container {
            position: relative;
            width: 100%;
            height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
        }

        .symbols-wrapper {
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            display: flex;
            gap: 40px;
            z-index: 5;
        }

        .symbol {
            font-size: 80px;
            font-weight: 900;
            display: flex;
            align-items: center;
            justify-content: center;
            width: 100px;
            height: 100px;
            border-radius: 50%;
            opacity: 0;
        }

        .checkmark {
            background: rgba(59, 210, 74, 0.2);
            color: #3bd24a;
        }

        .cross {
            background: rgba(255, 107, 107, 0.2);
            color: #FF6B6B;
        }

        .title-wrapper {
            font-size: 120px;
            font-weight: 900;
            letter-spacing: 8px;
            display: flex;
            gap: 15px;
            perspective: 1000px;
            position: relative;
            z-index: 10;
        }

        .letter {
            display: inline-block;
            position: relative;
            min-width: 60px;
            text-align: center;
            opacity: 0;
            filter: drop-shadow(0 10px 20px rgba(0, 0, 0, 0.3));
            transform-style: preserve-3d;
        }

        .letter:nth-child(1),
        .letter:nth-child(2),
        .letter:nth-child(3),
        .letter:nth-child(4),
        .letter:nth-child(5) {
            color: #3bd24a;
        }

        .letter:nth-child(6),
        .letter:nth-child(7) {
            color: #FF6B6B;
            text-transform: uppercase;
        }

        /* Персонаж */
        .character {
            position: absolute;
            bottom: 60px;
            right: 80px;
            z-index: 20;
            opacity: 0;
        }

        .alien {
            position: relative;
            width: 140px;
            height: 120px;
        }

        .alien-body {
            width: 120px;
            height: 120px;
            background: linear-gradient(135deg, #FF6B6B, #FF8E8E);
            border-radius: 30px;
            position: absolute;
            bottom: 0;
            left: 10px;
            box-shadow: 0 5px 15px rgba(255, 107, 107, 0.3);
        }

        .alien-eyes {
            position: absolute;
            top: 30px;
            left: 20px;
            display: flex;
            gap: 25px;
            z-index: 2;
        }

        .eye {
            width: 35px;
            height: 45px;
            background: white;
            border-radius: 50%;
            position: relative;
            overflow: hidden;
            border: 3px solid #333;
            box-shadow: 0 0 10px rgba(255, 255, 255, 0.5);
        }

        .eye-pupil {
            width: 20px;
            height: 20px;
            background: #222;
            border-radius: 50%;
            position: absolute;
            transition: all 0.1s ease;
        }

        .eye-sparkle {
            width: 8px;
            height: 8px;
            background: white;
            border-radius: 50%;
            position: absolute;
            top: 5px;
            right: 5px;
            opacity: 1;
        }

        .instruction-text {
            position: absolute;
            bottom: 150px;
            left: 50%;
            transform: translateX(-50%);
            color: #6B7280;
            font-size: 18px;
            font-weight: 500;
            opacity: 0;
            text-align: center;
            white-space: nowrap;
        }

        .floating-elements {
            position: absolute;
            width: 100%;
            height: 100%;
            pointer-events: none;
        }

        .floating-element {
            position: absolute;
            font-size: 24px;
            opacity: 0.1;
        }

        .particles-container {
            position: absolute;
            width: 100%;
            height: 100%;
            pointer-events: none;
        }

        .particle {
            position: absolute;
            width: 6px;
            height: 6px;
            border-radius: 50%;
            background: #3bd24a;
            opacity: 0;
        }

        .glow-orb {
            position: absolute;
            border-radius: 50%;
            filter: blur(80px);
            opacity: 0.3;
        }

        .orb-1 {
            width: 400px;
            height: 400px;
            background: rgba(255, 107, 107, 0.4);
            top: -100px;
            left: -100px;
        }

        .orb-2 {
            width: 300px;
            height: 300px;
            background: rgba(255, 215, 0, 0.4);
            bottom: -50px;
            right: -50px;
        }

        /* Область скролбару для анімації */
        .scroll-area {
            position: absolute;
            right: 0;
            top: 0;
            width: 20px;
            height: 100%;
            background: transparent;
            z-index: 100;
        }
    </style>
</head>
<body>
    <div class='container'>
        <div class='glow-orb orb-1'></div>
        <div class='glow-orb orb-2'></div>

        <!-- Область для анімації при скролі -->
        <div class='scroll-area' id='scrollArea'></div>

        <!-- Плаваючі елементи -->
        <div class='floating-elements'>
            <div class='floating-element' style='top: 20%; left: 10%'>✓</div>
            <div class='floating-element' style='top: 30%; right: 15%'>✕</div>
            <div class='floating-element' style='bottom: 40%; left: 20%'>●</div>
            <div class='floating-element' style='top: 60%; right: 25%'>■</div>
        </div>

        <!-- Частинки -->
        <div class='particles-container' id='particles'></div>

        <div class='symbols-wrapper'>
            <div class='symbol checkmark'>✓</div>
            <div class='symbol cross'>✕</div>
        </div>

        <div class='title-wrapper'>
            <span class='letter'>c</span>
            <span class='letter'>h</span>
            <span class='letter'>e</span>
            <span class='letter'>c</span>
            <span class='letter'>k</span>
            <span class='letter'>I</span>
            <span class='letter'>T</span>
        </div>

        <div class='instruction-text'>
            Обери функцію нижче для початку роботи
        </div>

        <!-- Новий персонаж -->
        <div class='character'>
            <div class='alien'>
                <div class='alien-body'>
                    <div class='alien-eyes'>
                        <div class='eye'>
                            <div class='eye-pupil'></div>
                            <div class='eye-sparkle'></div>
                        </div>
                        <div class='eye'>
                            <div class='eye-pupil'></div>
                            <div class='eye-sparkle'></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        // Ініціалізація GSAP
        gsap.registerPlugin();
        
        // Створення частинок
        function createParticles() {
            const container = document.getElementById('particles');
            const colors = ['#3bd24a', '#FF6B6B', '#667eea', '#764ba2'];
            
            for (let i = 0; i < 30; i++) {
                const particle = document.createElement('div');
                particle.className = 'particle';
                particle.style.background = colors[Math.floor(Math.random() * colors.length)];
                particle.style.left = Math.random() * 100 + '%';
                particle.style.top = Math.random() * 100 + '%';
                container.appendChild(particle);
            }
        }

        // Анімація блискіток очей
        function eyeSparkleAnimation() {
            gsap.to('.eye-sparkle', {
                opacity: 1,
                scale: 1.5,
                duration: 0.3,
                stagger: 0.2,
                repeat: -1,
                repeatDelay: 2,
                yoyo: true,
                ease: 'power2.inOut'
            });
        }

        // Слідкування за курсором
        function setupEyeTracking() {
            const pupils = document.querySelectorAll('.eye-pupil');
            const eyes = document.querySelectorAll('.eye');
            
            document.addEventListener('mousemove', (e) => {
                const mouseX = e.clientX;
                const mouseY = e.clientY;
                
                pupils.forEach((pupil, index) => {
                    const eye = eyes[index];
                    const eyeRect = eye.getBoundingClientRect();
                    const eyeCenterX = eyeRect.left + eyeRect.width / 2;
                    const eyeCenterY = eyeRect.top + eyeRect.height / 2;
                    
                    const deltaX = mouseX - eyeCenterX;
                    const deltaY = mouseY - eyeCenterY;
                    const distance = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
                    const maxDistance = 8;
                    
                    const angle = Math.atan2(deltaY, deltaX);
                    const moveX = Math.cos(angle) * Math.min(distance / 20, maxDistance);
                    const moveY = Math.sin(angle) * Math.min(distance / 20, maxDistance);
                    
                    pupil.style.transform = `translate(${moveX}px, ${moveY}px)`;
                });
            });
        }

        // Активна анімація радості при скролі
        function setupScrollAnimation() {
            const scrollArea = document.getElementById('scrollArea');
            const eyes = document.querySelectorAll('.eye');
            const alien = document.querySelector('.alien');
            
            let isScrolling = false;
            let scrollTimeout;
            let eyeAnimation;
            let headAnimation;

            scrollArea.addEventListener('mouseenter', () => {
                if (!isScrolling) {
                    isScrolling = true;

                    // Активне коливання головою вверх-вниз
                    headAnimation = gsap.to(alien, {
                        y: -15,
                        rotation: 3,
                        duration: 0.3,
                        repeat: -1,
                        yoyo: true,
                        ease: 'sine.inOut'
                    });

                    // Дуже активна та різка анімація очей вверх-вниз
                    eyeAnimation = gsap.to(eyes, {
                        y: -15,
                        duration: 0.1,
                        repeat: -1,
                        yoyo: true,
                        ease: 'power2.inOut',
                        stagger: 0.02
                    });

                    // Додаткова анімація - легке тремтіння від радості
                    gsap.to(alien, {
                        x: 'random(-1, 1)',
                        duration: 0.08,
                        repeat: 20,
                        ease: 'power1.inOut'
                    });
                }
                
                clearTimeout(scrollTimeout);
                scrollTimeout = setTimeout(() => {
                    isScrolling = false;
                    
                    // Зупинити всі анімації
                    if (eyeAnimation) eyeAnimation.kill();
                    if (headAnimation) headAnimation.kill();
                    
                    // Повернути все в нормальний стан
                    gsap.to(eyes, {
                        y: 0,
                        duration: 0.3,
                        ease: 'power2.out'
                    });
                    
                    gsap.to(alien, {
                        y: 0,
                        x: 0,
                        rotation: 0,
                        duration: 0.5,
                        ease: 'power2.out'
                    });
                }, 1000);
            });
        }

        // Основна анімація
        function startAnimation() {
            const tl = gsap.timeline();
            
            // Анімація символів
            tl.fromTo('.checkmark', 
                { scale: 0, rotation: -180, opacity: 0 },
                { scale: 1, rotation: 0, opacity: 1, duration: 1, ease: 'back.out(1.7)' }
            )
            .fromTo('.cross', 
                { scale: 0, rotation: 180, opacity: 0 },
                { scale: 1, rotation: 0, opacity: 1, duration: 1, ease: 'back.out(1.7)' },
                '-=0.5'
            )
            // Рух символів в сторони
            .to('.checkmark', {
                x: -280,
                rotation: 450,
                scale: 0.6,
                opacity: 0,
                duration: 1.5,
                ease: 'power2.inOut'
            })
            .to('.cross', {
                x: 280,
                rotation: -450,
                scale: 0.6,
                opacity: 0,
                duration: 1.5,
                ease: 'power2.inOut'
            }, '-=1.5')
            
            // Поява заголовка
            .to('.title-wrapper', {
                opacity: 1,
                scale: 1,
                duration: 0.8,
                ease: 'power2.out'
            })
            
            // Анімація букв з ефектом ""прибуття""
            .fromTo('.letter:nth-child(1)', 
                { opacity: 0, y: -500, scale: 0.5 },
                { opacity: 1, y: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' }
            )
            .fromTo('.letter:nth-child(2)', 
                { opacity: 0, x: -400, y: 400, rotation: -45, scale: 0.5 },
                { opacity: 1, x: 0, y: 0, rotation: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' },
                '-=0.9'
            )
            .fromTo('.letter:nth-child(3)', 
                { opacity: 0, x: -500, scale: 0.5 },
                { opacity: 1, x: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' },
                '-=0.9'
            )
            .fromTo('.letter:nth-child(4)', 
                { opacity: 0, x: 400, y: 350, rotation: 45, scale: 0.5 },
                { opacity: 1, x: 0, y: 0, rotation: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' },
                '-=0.9'
            )
            .fromTo('.letter:nth-child(5)', 
                { opacity: 0, x: 500, scale: 0.5 },
                { opacity: 1, x: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' },
                '-=0.9'
            )
            .fromTo('.letter:nth-child(6)', 
                { opacity: 0, x: -450, y: 450, rotation: -14, scale: 0.5 },
                { opacity: 1, x: 0, y: 0, rotation: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' },
                '-=0.9'
            )
            .fromTo('.letter:nth-child(7)', 
                { opacity: 0, x: -350, y: -450, rotation: 30, scale: 0.5 },
                { opacity: 1, x: 0, y: 0, rotation: 0, scale: 1, duration: 1.2, ease: 'elastic.out(1, 0.5)' },
                '-=0.9'
            )
            
            // Поява тексту інструкції
            .to('.instruction-text', {
                opacity: 1,
                y: 0,
                duration: 1,
                ease: 'power2.out'
            }, '-=0.5')
            
            // Поява персонажа
            .fromTo('.character', 
                { opacity: 0, x: 100, y: 50, scale: 0.8 },
                { 
                    opacity: 1, 
                    x: 0, 
                    y: 0, 
                    scale: 1,
                    duration: 1.5, 
                    ease: 'back.out(1.7)'
                },
                '-=0.5'
            )
            
            // Плавне коливання всього персонажа
            .to('.alien', {
                y: -8,
                duration: 1.5,
                ease: 'sine.inOut',
                yoyo: true,
                repeat: -1
            }, '-=1')
            
            // Анімація плаваючих елементів
            .fromTo('.floating-element', 
                { opacity: 0, scale: 0 },
                { 
                    opacity: 0.1, 
                    scale: 1, 
                    duration: 2, 
                    stagger: 0.2,
                    ease: 'sine.inOut',
                    yoyo: true,
                    repeat: -1
                },
                '-=1'
            )
            
            // Анімація частинок
            .fromTo('.particle', 
                { opacity: 0, scale: 0 },
                { 
                    opacity: 0.6, 
                    scale: 1,
                    duration: 2,
                    stagger: 0.1,
                    y: 'random(-100, 100)',
                    x: 'random(-50, 50)',
                    rotation: 'random(-360, 360)',
                    ease: 'sine.inOut',
                    yoyo: true,
                    repeat: -1
                },
                '-=1'
            )
            
            // Весела анімація для букви T
            .to('.letter:nth-child(7)', {
                y: -30,
                rotation: -5,
                duration: 0.8,
                ease: 'sine.inOut',
                yoyo: true,
                repeat: -1,
                delay: 1
            }, '-=2')
            
            // Анімація світлових сфер
            .to('.orb-1', {
                x: 30,
                y: -30,
                duration: 4,
                ease: 'sine.inOut',
                yoyo: true,
                repeat: -1
            }, '-=3')
            .to('.orb-2', {
                x: -20,
                y: 20,
                duration: 5,
                ease: 'sine.inOut',
                yoyo: true,
                repeat: -1
            }, '-=4');

            return tl;
        }

        // Запуск анімації після завантаження
        document.addEventListener('DOMContentLoaded', function() {
            createParticles();
            const animation = startAnimation();
            
            // Запуск анімацій очей
            eyeSparkleAnimation();
            
            // Налаштування слідкування за курсором
            setupEyeTracking();
            setupScrollAnimation();
            
            // Додатковий ефект - хвиля при наведенні на букви
            document.querySelectorAll('.letter').forEach(letter => {
                letter.addEventListener('mouseenter', () => {
                    gsap.to(letter, {
                        scale: 1.3,
                        rotation: 'random(-10, 10)',
                        duration: 0.3,
                        ease: 'back.out(1.7)'
                    });
                });
                
                letter.addEventListener('mouseleave', () => {
                    gsap.to(letter, {
                        scale: 1,
                        rotation: 0,
                        duration: 0.3,
                        ease: 'back.out(1.7)'
                    });
                });
            });

            // Інтерактивність для персонажа
            document.querySelector('.character').addEventListener('mouseenter', () => {
                gsap.to('.alien', {
                    scale: 1.1,
                    duration: 0.3,
                    ease: 'back.out(1.7)'
                });
            });
            
            document.querySelector('.character').addEventListener('mouseleave', () => {
                gsap.to('.alien', {
                    scale: 1,
                    duration: 0.3,
                    ease: 'back.out(1.7)'
                });
            });
        });
    </script>
</body>
</html>";

                HeaderWebView.NavigateToString(htmlContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження анімації: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool EnsureAuthenticated()
        {
            if (_auth_service_is_authenticated()) return true;

            using var scope = App.AppHost?.Services.CreateScope();
            if (scope == null) return false;

            try
            {
                var login = scope.ServiceProvider.GetRequiredService<LoginWindow>();
                login.Owner = Window.GetWindow(this);
                var res = login.ShowDialog();

                var rootAuth = App.AppHost?.Services.GetService<IAuthService>();
                if (rootAuth != null && rootAuth.IsAuthenticated)
                    return true;

                return res == true && (_authService != null && _authService.IsAuthenticated);
            }
            finally
            {
                // scope disposed by using
            }
        }

        private bool _auth_service_is_authenticated()
        {
            return _authService != null && _authService.IsAuthenticated;
        }

        private void OpenRegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            var reg = _serviceProvider?.GetService<RegisterWindow>();
            if (reg != null)
            {
                reg.Owner = Window.GetWindow(this);
                reg.ShowDialog();
            }
        }

        private void OpenLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var login = _serviceProvider?.GetService<LoginWindow>();
            if (login != null)
            {
                login.Owner = Window.GetWindow(this);
                login.ShowDialog();
            }
        }

        private void OpenProzorro_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!EnsureAuthenticated()) return;

                var wnd = _serviceProvider?.GetService<Check_IT.ProzorroWindow>() ?? App.AppHost?.Services.GetService<Check_IT.ProzorroWindow>();
                if (wnd != null)
                {
                    wnd.Owner = Window.GetWindow(this);
                    wnd.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Window.GetWindow(this), $"Не вдалося відкрити вікно: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!EnsureAuthenticated()) return;

                var wnd = _serviceProvider?.GetService<Check_IT.PrivateTenderWindow>() ?? App.AppHost?.Services.GetService<Check_IT.PrivateTenderWindow>();
                if (wnd != null)
                {
                    wnd.Owner = Window.GetWindow(this);
                    wnd.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Window.GetWindow(this), $"Не вдалося відкрити вікно: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}