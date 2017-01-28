---
layout: default
images:
  - image_path: https://github.com/achorein/letsshootemup-unity/raw/master/docs/assets/images/screen-01.png
  - image_path: https://github.com/achorein/letsshootemup-unity/raw/master/docs/assets/images/screen-02.png
  - image_path: https://github.com/achorein/letsshootemup-unity/raw/master/docs/assets/images/screen-03.png
---

<p>
    <img src="https://github.com/achorein/letsshootemup-unity/raw/master/docs/assets/images/logo-1024x500.png" height="200"/>
</p>

Embark in your ship and engage the fight against dreadful space pirates in a never-ending combat for your survival.<br/>
The game is fast, addictive, hard and action-packed.

<ul>
    {% for image in page.images %}
    <li><a href="{{ image.image_path }}"><img src="{{ image.image_path }}" height="200"/></a></li>
    {% endfor %}
</ul>


